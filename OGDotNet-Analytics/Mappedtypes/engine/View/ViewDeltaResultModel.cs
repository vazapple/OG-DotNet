﻿//-----------------------------------------------------------------------
// <copyright file="ViewDeltaResultModel.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Diagnostics.CodeAnalysis;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Builders.ViewResultModel;

namespace OGDotNet.Mappedtypes.engine.View
{
    [FudgeSurrogate(typeof(InMemoryViewDeltaResultModelBuilder))]

    [SuppressMessage("Microsoft.StyleCop.CSharp.NamingRules", "SA1302:InterfaceNamesMustBeginWithI", Justification = "Name used for fudge mapping")]
    // ReSharper disable InconsistentNaming
    public interface ViewDeltaResultModel : IViewResultModel
    // ReSharper restore InconsistentNaming
    {
        DateTimeOffset PreviousResultTimestamp { get; }
    }
}