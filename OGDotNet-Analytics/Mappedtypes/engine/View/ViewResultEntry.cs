﻿//-----------------------------------------------------------------------
// <copyright file="ViewResultEntry.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Mappedtypes.Engine.View
{
    public class ViewResultEntry
    {
        private readonly string _calculationConfiguration;
        private readonly ComputedValue _computedValue;

        public ViewResultEntry(string calculationConfiguration, ComputedValue computedValue)
        {
            _calculationConfiguration = calculationConfiguration;
            _computedValue = computedValue;
        }

        public string CalculationConfiguration
        {
            get { return _calculationConfiguration; }
        }

        public ComputedValue ComputedValue
        {
            get { return _computedValue; }
        }
    }
}