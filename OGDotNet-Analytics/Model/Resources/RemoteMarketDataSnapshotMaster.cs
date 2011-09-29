﻿//-----------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotMaster.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Mappedtypes.Id;
using OGDotNet.Mappedtypes.Master;
using OGDotNet.Mappedtypes.Master.MarketDataSnapshot;
using OGDotNet.Mappedtypes.Util;

namespace OGDotNet.Model.Resources
{
    public class RemoteMarketDataSnapshotMaster : IMaster<MarketDataSnapshotDocument>
    {
        private readonly RestTarget _restTarget;
        private readonly string _activeMQSpec;
        private readonly OpenGammaFudgeContext _fudgeContext;

        public RemoteMarketDataSnapshotMaster(RestTarget restTarget, string activeMQSpec, OpenGammaFudgeContext fudgeContext)
        {
            _restTarget = restTarget;
            _fudgeContext = fudgeContext;
            _activeMQSpec = activeMQSpec;
        }

        public SearchResult<MarketDataSnapshotDocument> Search(string name, PagingRequest pagingRequest)
        {
            var request = new MarketDataSnapshotSearchRequest(name, pagingRequest);
            return Search(request);
        }

        public SearchResult<MarketDataSnapshotDocument> Search(MarketDataSnapshotSearchRequest request)
        {
            return _restTarget.Resolve("search").Post<SearchResult<MarketDataSnapshotDocument>>(request);
        }

        public MarketDataSnapshotDocument Add(MarketDataSnapshotDocument document)
        {
            return PostDefinition(document, "add");
        }

        public MarketDataSnapshotDocument Update(MarketDataSnapshotDocument document)
        {
            if (document.UniqueId == null)
            {
                throw new ArgumentException();
            }
            return PutDefinition(document, "snapshots", document.UniqueId.ToString());
        }

        private MarketDataSnapshotDocument PutDefinition(MarketDataSnapshotDocument document, params string[] pathParts)
        {
            return ReqDefinition((t, d, f) => t.Put<UniqueId>(d, f), document, pathParts);
        }
        private MarketDataSnapshotDocument PostDefinition(MarketDataSnapshotDocument document, params string[] pathParts)
        {
            return ReqDefinition((t, d, f) => t.Post<UniqueId>(d, f), document, pathParts);
        }

        private MarketDataSnapshotDocument ReqDefinition(Func<RestTarget, MarketDataSnapshotDocument, string, UniqueId> reqFunc, MarketDataSnapshotDocument document, params string[] pathParts)
        {
            var target = pathParts.Aggregate(_restTarget, (r, p) => r.Resolve(p));
            var uid = reqFunc(target, document, "uniqueId");
            if (uid == null)
            {
                throw new ArgumentException("No UID returned");
            }

            document.UniqueId = uid;
            document.Snapshot.UniqueId = uid;

            return document;
        }

        public RemoteChangeManger ChangeManager
        {
            get
            {
                return new RemoteChangeManger(_restTarget.Resolve("changeManager"), _activeMQSpec, _fudgeContext);
            }
        }

        public MarketDataSnapshotDocument Get(UniqueId uniqueId)
        {
            var resp = _restTarget.Resolve("snapshots").Resolve(uniqueId.ToString()).Get<MarketDataSnapshotDocument>();
            if (resp == null || resp.UniqueId == null || resp.Snapshot == null)
            {
                throw new ArgumentException("Not found", "uniqueId");
            }
            return resp;
        }

        public void Remove(UniqueId uniqueId)
        {
            _restTarget.Resolve("snapshots").Resolve(uniqueId.ToString()).Delete();
        }
        //TODO correct
    }
}