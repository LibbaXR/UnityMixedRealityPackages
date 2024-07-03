// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using ml.zi;

namespace MagicLeap.ZI
{
    internal abstract class ViewModel
    {
        public event Action OnSessionStarted;
        public event Action OnSessionStopped;

        public bool IsSessionRunning { get; private set; }
        public bool IsDeviceMode => ZIBridge.IsDeviceMode;

        protected static ZIBridge Bridge => ZIBridge.Instance;

        public virtual void Initialize()
        {
            Bridge.OnSessionConnectedChanged += SessionConnectionStatusChanged;
            SessionConnectionStatusChanged(Bridge.IsConnected);
        }

        public virtual void UnInitialize()
        {
            Bridge.OnSessionConnectedChanged -= SessionConnectionStatusChanged;
        }

        public virtual void Update()
        {
        }
        
        protected virtual void SessionStarted()
        {
            OnSessionStarted?.Invoke();
        }

        protected virtual void SessionConnectionStatusChanged(bool connectionStatus)
        {
            if (AreRequiredModulesConnected() && !IsSessionRunning)
            {
                SessionStarted();
                IsSessionRunning = true;
            }
            else if (!AreRequiredModulesConnected() && IsSessionRunning)
            {
                SessionStopped();
                IsSessionRunning = false;
            }
        }

        protected abstract bool AreRequiredModulesConnected();

        protected virtual void SessionStopped()
        {
            OnSessionStopped?.Invoke();
        }
    }
}
