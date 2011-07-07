﻿//-----------------------------------------------------------------------
// <copyright file="MarketDataSnapshotProcessor.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using OGDotNet.Mappedtypes.Core.marketdatasnapshot;
using OGDotNet.Mappedtypes.engine.view;
using OGDotNet.Mappedtypes.financial.analytics.ircurve;
using OGDotNet.Mappedtypes.financial.model.interestrate.curve;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master.marketdatasnapshot;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.math.curve;
using OGDotNet.Model.Context.MarketDataSnapshot;
using OGDotNet.Model.Resources;
using OGDotNet.Utils;

namespace OGDotNet.Model.Context
{
    /// <summary>
    /// <para>This class handles creating and mutating snapshots based on Views and live data</para>
    /// <list type="table">
    /// <item>TODO: this implementation probably shouldn't be client side</item>
    /// </list>
    /// </summary>
    public class MarketDataSnapshotProcessor : DisposableBase
    {
        private readonly ManageableMarketDataSnapshot _snapshot;
        private readonly RemoteMarketDataSnapshotMaster _marketDataSnapshotMaster;
        private readonly LiveDataStream _liveDataStream;
        private readonly SnapshotDataStream _snapshotDataStream;

        private readonly object _snapshotUidLock = new object();
        private readonly RemoteClient _remoteClient;
        private UniqueIdentifier _temporarySnapshotUid;

        internal static MarketDataSnapshotProcessor Create(RemoteEngineContext context, ViewDefinition definition, CancellationToken ct)
        {
            var liveDataStream = new LiveDataStream(definition.Name, context);
            var snapshot = liveDataStream.GetNewSnapshotForUpdate(ct);
            
            return new MarketDataSnapshotProcessor(snapshot, context, liveDataStream);
        }

        internal MarketDataSnapshotProcessor(RemoteEngineContext remoteEngineContext, ManageableMarketDataSnapshot snapshot)
            : this(snapshot, remoteEngineContext, new LiveDataStream(snapshot.BasisViewName, remoteEngineContext))
        {
        }

        private MarketDataSnapshotProcessor(ManageableMarketDataSnapshot snapshot, RemoteEngineContext remoteEngineContext, LiveDataStream liveDataStream)
        {
            _snapshot = snapshot;
            _remoteClient = remoteEngineContext.CreateUserClient();
            _marketDataSnapshotMaster = _remoteClient.MarketDataSnapshotMaster;
            _liveDataStream = liveDataStream;
            _temporarySnapshotUid = _marketDataSnapshotMaster.Add(new MarketDataSnapshotDocument(null, GetShallowCloneSnapshot())).UniqueId;
            _snapshotDataStream = new SnapshotDataStream(_snapshot.BasisViewName, remoteEngineContext, _temporarySnapshotUid.ToLatest(), _liveDataStream);
            _snapshot.PropertyChanged += SnapshotPropertyChanged;
        }

        private void SnapshotPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "BasisViewName")
            {
                _liveDataStream.BasisViewName = _snapshot.BasisViewName;
            }
        }

        public ManageableMarketDataSnapshot Snapshot
        {
            get
            {
                CheckDisposed(); 
                return _snapshot;
            }
        }

        public UpdateAction<ManageableMarketDataSnapshot> PrepareUpdate(CancellationToken ct = default(CancellationToken))
        {
            return Snapshot.PrepareUpdateFrom(GetNewSnapshotForUpdate(ct));
        }

        public ManageableMarketDataSnapshot GetNewSnapshotForUpdate(CancellationToken ct = default(CancellationToken))
        {
            return _liveDataStream.GetNewSnapshotForUpdate(ct);
        }

        public LiveDataStream LiveDataStream
        {
            get { return _liveDataStream; }
        }

        public Dictionary<YieldCurveKey, Tuple<YieldCurve, InterpolatedYieldCurveSpecificationWithSecurities, NodalDoublesCurve>> GetYieldCurves(CancellationToken ct = default(CancellationToken))
        {
            CheckDisposed();

            lock (_snapshotUidLock)
            {
                ManageableMarketDataSnapshot shallowClone = GetShallowCloneSnapshot();
                shallowClone.UniqueId = _temporarySnapshotUid;
                var snapshot = _marketDataSnapshotMaster.Update(new MarketDataSnapshotDocument(_temporarySnapshotUid, shallowClone));
                _temporarySnapshotUid = snapshot.UniqueId;

                var waitFor = _marketDataSnapshotMaster.Get(_temporarySnapshotUid).CorrectionFromInstant;
                return _snapshotDataStream.GetYieldCurves(waitFor, ct);
            }
        }

        private ManageableMarketDataSnapshot GetShallowCloneSnapshot()
        {
            return new ManageableMarketDataSnapshot(_snapshot.BasisViewName, _snapshot.GlobalValues, _snapshot.YieldCurves, _snapshot.VolatilityCubes, _snapshot.VolatilitySurfaces, _snapshot.UniqueId)
                       {
                           Name = string.Format("{0}-{1}-{2}", typeof(MarketDataSnapshotProcessor).Name, Guid.NewGuid(), _snapshot.BasisViewName),
                           UniqueId = null
                       };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _snapshot.PropertyChanged -= SnapshotPropertyChanged;
                _liveDataStream.Dispose();
                _snapshotDataStream.Dispose();
                _marketDataSnapshotMaster.Remove(_temporarySnapshotUid.ToLatest());
                _remoteClient.Dispose();
            }
        }
    }
}
