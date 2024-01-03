// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed partial class ZIBridge
    {
        public class PausingModuleWrapper<T, T1> : ModuleWrapper<T, T1> where T : HandleBase, IHandleSessions, IHandlePausing, IHandleChanges<T1> where T1 : Enum
        {
            protected IHandlePausing HandlePausing => Handle;
            
            private readonly HashSet<object> listeners = new();

            public void StartListening(object listener)
            {
                listeners.Add(listener);
            }

            public void StopListening(object listener)
            {
                listeners.Remove(listener);
            }

            private void CheckStatus()
            {
                bool isModulePaused = HandlePausing.IsPaused();

                if (!EditorWindowFocusUtility.AppFocused)
                {
                    if (!isModulePaused)
                        HandlePausing.Pause();

                    return;
                }

                switch (listeners.Count)
                {
                    case > 0 when isModulePaused:
                        HandlePausing.Resume();
                        break;

                    case <= 0 when !isModulePaused:
                        HandlePausing.Pause();
                        break;
                }
            }
            
            public override void UpdateStatus()
            {
                base.UpdateStatus();
                
                if (IsConnected) 
                    CheckStatus();
            }
        }

        public class ModuleWrapper<T, T1> where T : HandleBase, IHandleSessions, IHandleChanges<T1> where T1 : Enum
        {
            private readonly ProgressMonitor sessionConnectingMonitor = ProgressMonitorDisplay.Show($"Connecting {typeof(T)} to session.", "Magic Leap App Simulator");
            
            public Action<T1> OnTakeChanges;

            public event Action<bool> OnHandleConnectionChanged;
            
            public bool IsHandleConnected
            {
                get
                {
                    if (HandleBase == null)
                        return false;
                    
                    return HandleBase.GetHandle() != 0 && HandleSessions.SessionConnected();
                }
            }

            public T Handle;
            protected bool isExact = false ;
            protected bool isConnected = false;

            protected bool IsConnected
            {
                get => isConnected;
                set
                {
                    if (isConnected == value)
                        return;

                    isConnected = value;
                    OnHandleConnectionChanged?.Invoke(isConnected);
                }
            }

            public virtual void ConnectHandle(T handleBase)
            {
                Handle = handleBase;
                isExact = TrySessionConnect();
            }

            protected HandleBase HandleBase => Handle;
            protected IHandleSessions HandleSessions => Handle;
            protected IHandleChanges<T1> HandleChanges => Handle;
            
            public virtual void UpdateStatus()
            {
                if (!isExact)
                    return;

                IsConnected = IsHandleConnected;
            }

            public void CheckChanges()
            {
                UpdateStatus();
                
                if (!IsHandleConnected)
                    return;

                var changes = HandleChanges.TakeChanges();

                var hasFlag = changes.Equals(default(T1));
                if (!hasFlag)
                {
                    OnTakeChanges?.Invoke(changes);
                }
            }

            public virtual bool TrySessionConnect()
            {
                try
                {
                    HandleSessions.SessionConnect(sessionConnectingMonitor);
                    return HandleBase.GetHandle() != 0 && HandleSessions.SessionConnected();
                }
                catch (ResultIsErrorException exception)
                {
                    if (exception.Result is Result.DeviceNotFound or Result.NotSupportedInSession)
                        return false;
                    
                    Debug.LogWarning($"{typeof(T)} not connected with result {exception.Result}");
                    throw;
                }
            }

            public void TrySessionDisconnect()
            {
                if (!IsHandleConnected)
                {
                    IsConnected = false;
                    return;
                }
                try
                {
                    if (HandleBase.GetHandle() != 0)
                    {
                        HandleSessions.SessionDisconnect();
                    }
                }
                catch (ResultIsErrorException exception)
                {
                    Debug.LogWarning($"{typeof(T)} disconnecting failed with result {exception.Result}");
                }
                finally
                {
                    HandleBase.SetHandle(0);
                    IsConnected = false;

                }
            }
        }
    }
}