﻿//-----------------------------------------------------------------------
// <copyright file="ValueSpecificationBuilder.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System;
using Fudge;
using Fudge.Serialization;
using OGDotNet.Mappedtypes.Engine;
using OGDotNet.Mappedtypes.Engine.Value;

namespace OGDotNet.Builders
{
    class ValueSpecificationBuilder : BuilderBase<ValueSpecification>
    {
        public ValueSpecificationBuilder(FudgeContext context, Type type) : base(context, type)
        {
        }

        protected override void SerializeImpl(ValueSpecification obj, IAppendingFudgeFieldContainer msg, IFudgeSerializer serializer)
        {
            new ComputationTargetSpecificationBuilder(serializer.Context, typeof(ComputationTargetSpecification)).Serialize(obj.TargetSpecification, msg, serializer);

            serializer.WriteInline(msg, "properties", obj.Properties);
            msg.Add("valueName", obj.ValueName);
        }

        public override ValueSpecification DeserializeImpl(IFudgeFieldContainer msg, IFudgeDeserializer deserializer)
        {
            string valueName = null;
            ValueProperties properties = null;

            foreach (var fudgeField in msg)
            {
                switch (fudgeField.Name)
                {
                    case "valueName":
                        valueName = (string) fudgeField.Value;
                        break;
                    case "properties":
                        properties = deserializer.FromField<ValueProperties>(fudgeField);
                        break;
                    default:
                        break;
                }
            }
            
            var targetSpecification = new ComputationTargetSpecificationBuilder(deserializer.Context, typeof(ComputationTargetSpecification)).DeserializeImpl(msg, deserializer); //Can't register twice

            return new ValueSpecification(valueName, targetSpecification, properties);
        }
    }
}
