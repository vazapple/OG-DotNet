﻿//-----------------------------------------------------------------------
// <copyright file="RemoteMarketDataSnapshotter.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Core.MarketDataSnapshot.Impl;
using OGDotNet.Mappedtypes.Engine.View.Calc;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Model.Resources
{
    public class RemoteMarketDataSnapshotter
    {
        private readonly RestTarget _rest;

        public RemoteMarketDataSnapshotter(RestTarget rest)
        {
            _rest = rest;
        }

        public ManageableMarketDataSnapshot CreateSnapshot(RemoteViewClient client, IViewCycle cycle)
        {
            UniqueId clientId = client.GetUniqueId();
            UniqueId cycleId = cycle.UniqueId;

            var createTarget = _rest.Resolve("create", clientId.ToString(), cycleId.ToString());
            return createTarget.Get<ManageableMarketDataSnapshot>();
        }
    }
}
