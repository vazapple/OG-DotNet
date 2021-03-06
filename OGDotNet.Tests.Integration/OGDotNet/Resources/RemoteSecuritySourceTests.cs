﻿//-----------------------------------------------------------------------
// <copyright file="RemoteSecuritySourceTests.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Linq;
using OGDotNet.Mappedtypes.Id;
using Xunit;
using FactAttribute = OGDotNet.Tests.Integration.Xunit.Extensions.FactAttribute;

namespace OGDotNet.Tests.Integration.OGDotNet.Resources
{
    public class RemoteSecuritySourceTests : TestWithContextBase
    {
        [Fact]
        public void CanCreate()
        {
            var remoteSecuritySource = Context.SecuritySource;
            Assert.NotNull(remoteSecuritySource);
        }

        [Fact]
        public void CanDoEmptyBundleMultiQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurities(new ExternalIdBundle()));
        }

        [Fact]
        public void CanDoEmptyBundleSingleQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            Assert.Throws<ArgumentException>(() => remoteSecuritySource.GetSecurity(new ExternalIdBundle()));
        }

        [Fact]
        public void CanDoMissingUidQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var security = remoteSecuritySource.GetSecurity(StupidUid);
            Assert.Null(security);
        }

        [Fact]
        public void CanDoEmptyMissingBundleQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var collection = remoteSecuritySource.GetSecurities(new ExternalIdBundle(StupidIdentifier));
            Assert.Empty(collection);
        }

        [Fact]
        public void CanDoSingleBundleQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var collection = remoteSecuritySource.GetSecurities(new ExternalIdBundle(ExternalId.Create("BLOOMBERG_TICKER", "AAPL US Equity")));
            Assert.Equal(1, collection.Count);
            Assert.True(collection.First().Name.IndexOf("Apple", StringComparison.InvariantCultureIgnoreCase) >= 0);
        }

        [Fact]
        public void CanDoMultipleBundleQuery()
        {
            var remoteSecuritySource = Context.SecuritySource;
            var collection = remoteSecuritySource.GetSecurities(new ExternalIdBundle(ExternalId.Create("BLOOMBERG_TICKER", "AAPL US Equity"), ExternalId.Create("BLOOMBERG_TICKER", "MSFT US Equity")));
            Assert.Equal(2, collection.Count);
        }

        private static UniqueId StupidUid
        {
            get { return UniqueId.Create("xxx", "xxx"); }
        }

        private static ExternalId StupidIdentifier
        {
            get { return new ExternalId("xxx", "xxx"); }
        }
    }
}
