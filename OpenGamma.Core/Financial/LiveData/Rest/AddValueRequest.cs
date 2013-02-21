﻿// --------------------------------------------------------------------------------------------------------------------
// <copyright file="AddValueRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//   Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//   
//   Please see distribution for license.
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using OpenGamma.Engine.Value;
using OpenGamma.Id;

namespace OpenGamma.Financial.LiveData.Rest
{
    public class AddValueRequest
    {
        public ValueRequirement ValueRequirement { get; set; }

        public ExternalId Identifier { get; set; }

        public string ValueName { get; set; }

        public object Value { get; set; }
    }
}
