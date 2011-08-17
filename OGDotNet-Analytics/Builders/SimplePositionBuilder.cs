﻿//-----------------------------------------------------------------------
// <copyright file="SimplePositionBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using System.Collections.Generic;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Core.Position;
using OGDotNet.Mappedtypes.Core.Position.Impl;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Builders
{
    class SimplePositionBuilder : BuilderBase<IPosition>
    {
        public SimplePositionBuilder(FudgeContext context, Type type)
            : base(context, type)
        {
        }

        public override IPosition DeserializeImpl(IFudgeFieldContainer ffc, IFudgeDeserializer deserializer)
        {
            var id = ffc.GetValue<string>("uniqueId");
            var secKey = deserializer.FromField<ExternalIdBundle>(ffc.GetByName("securityKey"));
            var quant = ffc.GetValue<string>("quantity");
            var trades = deserializer.FromField<IList<ITrade>>(ffc.GetByName("trades")) ?? new List<ITrade>();
            return new SimplePosition(id == null ? null : UniqueId.Parse(id), long.Parse(quant), secKey, trades);
        }
    }
}