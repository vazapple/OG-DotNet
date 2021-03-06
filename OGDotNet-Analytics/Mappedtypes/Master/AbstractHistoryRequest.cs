﻿//-----------------------------------------------------------------------
// <copyright file="AbstractHistoryRequest.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------
using OGDotNet.Mappedtypes.Id;

namespace OGDotNet.Mappedtypes.Master
{
    public abstract class AbstractHistoryRequest
    {
        //TODO this
        private readonly ObjectId _objectId;

        protected AbstractHistoryRequest(ObjectId objectId)
        {
            _objectId = objectId;
        }

        public ObjectId ObjectId
        {
            get { return _objectId; }
        }
    }
}
