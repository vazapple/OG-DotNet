﻿//-----------------------------------------------------------------------
// <copyright file="IChangeListener.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
namespace OGDotNet.Mappedtypes.Core.Change
{
    public interface IChangeListener
    {
        void EntityChanged(ChangeEvent changeEvent);
    }
}