﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ExecutionOptions.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using Fudge.Serialization;

using OpenGamma.Engine.MarketData.Spec;
using OpenGamma.Fudge;
using OpenGamma.Id;

namespace OpenGamma.Engine.View.Execution
{
    [FudgeSurrogate(typeof(ExecutionOptionsBuilder))]
    public class ExecutionOptions : IViewExecutionOptions
    {
        public static IViewExecutionOptions RealTime
        {
            get
            {
                return GetRealTime(null);
            }
        }

        public static IViewExecutionOptions GetRealTime(string liveDataSource)
        {
            var liveMarketDataSpecification = new LiveMarketDataSpecification(liveDataSource);
            var marketDataSpecifications = new List<MarketDataSpecification> {liveMarketDataSpecification};
            return new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled, null, new ViewCycleExecutionOptions(default(DateTimeOffset), marketDataSpecifications));
        }

        public static IViewExecutionOptions SingleCycle
        {
            get { return GetSingleCycle(GetDefaultMarketDataSpec()); }
        }

        public static LiveMarketDataSpecification GetDefaultMarketDataSpec()
        {
            return new LiveMarketDataSpecification();
        }

        public static IViewExecutionOptions GetSingleCycle(MarketDataSpecification marketDataSpecification)
        {
            return GetSingleCycle(DateTimeOffset.Now, marketDataSpecification);
        }

        public static IViewExecutionOptions GetSingleCycle(DateTimeOffset valuationTime, MarketDataSpecification marketDataSpecification)
        {
            var marketDataSpecifications = new List<MarketDataSpecification> {marketDataSpecification};
            var viewCycleExecutionOptions = new ViewCycleExecutionOptions(valuationTime, marketDataSpecifications);
            return Batch(ArbitraryViewCycleExecutionSequence.Create(viewCycleExecutionOptions));
        }

        public static IViewExecutionOptions Batch(IViewCycleExecutionSequence cycleExecutionSequence)
        {
            return Batch(cycleExecutionSequence, null);
        }

        public static IViewExecutionOptions Batch(IViewCycleExecutionSequence cycleExecutionSequence, ViewCycleExecutionOptions defaultCycleOptions)
        {
            return new ExecutionOptions(cycleExecutionSequence, ViewExecutionFlags.RunAsFastAsPossible | ViewExecutionFlags.AwaitMarketData, defaultExecutionOptions: defaultCycleOptions);
        }

        public static IViewExecutionOptions GetCompileOnly()
        {
            return GetCompileOnly(ArbitraryViewCycleExecutionSequence.Create(DateTimeOffset.Now));
        }

        public static IViewExecutionOptions GetCompileOnly(IViewCycleExecutionSequence cycleExecutionSequence)
        {
            return new ExecutionOptions(cycleExecutionSequence, ViewExecutionFlags.CompileOnly);
        }

        public static IViewExecutionOptions Snapshot(UniqueId snapshotIdentifier)
        {
            var userMarketDataSpecification = new UserMarketDataSpecification(snapshotIdentifier);
            var marketDataSpecifications = new List<MarketDataSpecification> {userMarketDataSpecification};
            return new ExecutionOptions(new InfiniteViewCycleExecutionSequence(), ViewExecutionFlags.TriggersEnabled, defaultExecutionOptions: new ViewCycleExecutionOptions(default(DateTimeOffset), marketDataSpecifications));
        }

        private readonly IViewCycleExecutionSequence _executionSequence;
        private readonly ViewExecutionFlags _flags;
        private readonly int? _maxSuccessiveDeltaCycles;
        private readonly ViewCycleExecutionOptions _defaultExecutionOptions;
        private readonly VersionCorrection _versionCorrection;

        public ExecutionOptions(IViewCycleExecutionSequence executionSequence, ViewExecutionFlags flags, int? maxSuccessiveDeltaCycles = null, ViewCycleExecutionOptions defaultExecutionOptions = null, VersionCorrection versionCorrection = null)
        {
            _executionSequence = executionSequence;
            _flags = flags;
            _maxSuccessiveDeltaCycles = maxSuccessiveDeltaCycles;
            _defaultExecutionOptions = defaultExecutionOptions;
            _versionCorrection = versionCorrection ?? VersionCorrection.Latest;
        }

        public IViewCycleExecutionSequence ExecutionSequence
        {
            get { return _executionSequence; }
        }

        public ViewExecutionFlags Flags
        {
            get { return _flags; }
        }

        public ViewCycleExecutionOptions DefaultExecutionOptions
        {
            get { return _defaultExecutionOptions; }
        }

        public int? MaxSuccessiveDeltaCycles
        {
            get { return _maxSuccessiveDeltaCycles; }
        }

        public VersionCorrection VersionCorrection
        {
            get { return _versionCorrection; }
        }
    }
}