﻿//-----------------------------------------------------------------------
// <copyright file="SimplePortfolioNode.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using System.Collections.Generic;
using Fudge.Serialization;
using OGDotNet.Builders;
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Core.Position.Impl
{
    [FudgeSurrogate(typeof(SimplePortfolioNodeBuilder))]
    internal class SimplePortfolioNode : PortfolioNode
    {
        public SimplePortfolioNode(UniqueId identifier, string name, IList<PortfolioNode> subNodes, IList<IPosition> positions) : base(identifier, name, subNodes, positions)
        {
        }
    }
}
