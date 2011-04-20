﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ICompiledViewDefinition.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.engine.value;
using OGDotNet.Mappedtypes.engine.Value;
using OGDotNet.Mappedtypes.engine.view;

namespace OGDotNet.Mappedtypes.engine.View.compilation
{
    public interface ICompiledViewDefinition
    {
        ViewDefinition ViewDefinition { get; }

        IPortfolio Portfolio { get; }

        Dictionary<ValueRequirement, ValueSpecification> LiveDataRequirements { get; }

        string[] OutputValueNames { get; }

        //TODO IEnumerable<ComputationTarget> ComputationTargets

        string[] SecurityTypes { get; }

        //TODO ValidFrom, ValidTo
        DateTimeOffset EarliestValidity { get; }
        DateTimeOffset LatestValidity { get; }
    }
}