﻿//-----------------------------------------------------------------------
// <copyright file="HeartbeatSender.cs" company="OpenGamma Inc. and the OpenGamma group of companies">
//     Copyright © 2009 - present by OpenGamma Inc. and the OpenGamma group of companies
//
//     Please see distribution for license.
// </copyright>
//-----------------------------------------------------------------------

using System;
using System.Threading;
using OGDotNet.Utils;

namespace OGDotNet.Model.Resources
{
    public class HeartbeatSender : DisposableBase
    {
        private readonly ManualResetEvent _disposeEvent = new ManualResetEvent(false);
        private readonly TimeSpan _period;
        private readonly RestTarget _heartbeatRest;
        private readonly RegisteredWaitHandle _registeredWaitHandle;

        public HeartbeatSender(TimeSpan period, RestTarget heartbeatRest)
        {
            _period = period;
            _heartbeatRest = heartbeatRest;

            _registeredWaitHandle = ThreadPool.RegisterWaitForSingleObject(_disposeEvent, SendHeartBeats, null, _period, false);
        }

        private void SendHeartBeats(object context, bool timedOut)
        {
            if (!timedOut)
            {
                throw new ArgumentException("Handle should never be set");
            }
            SendHeartbeat();
        }

        private void SendHeartbeat()
        {
            _heartbeatRest.Post();
        }

        protected override void Dispose(bool disposing)
        {
            _registeredWaitHandle.Unregister(_disposeEvent);
            _disposeEvent.Dispose();
        }
    }
}