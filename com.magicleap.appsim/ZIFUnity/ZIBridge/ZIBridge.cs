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
using System.Data;
using System.Runtime.CompilerServices;
using ml.zi;
using UnityEditor.XR.MagicLeap;
using UnityEngine;
using UnityEditor;

namespace MagicLeap.ZI
{
    /**
     * Coordinates common data with the zifUnity plugin.
     * 
     * The instance contains fixed ml.zi.* objects which
     * can be used form any client.  Those objects will
     * always reference the active ZIHandle on the C API side.
     * 
     * Currently this uses a background thread to see if the
     * Session handle has changed on the plugin side, and
     * if so, synchronizes the other handles with it.
     */
    internal sealed partial class ZIBridge
    {
        public static readonly ZIBridge instance = new();

        public static ZIBridge Instance => instance;

        // This version is only safe from the main thread.
        // Use ZISettings.Instance.EnableDebugLogging otherwise.
        internal static bool Debugging => EditorPrefs.GetBool("ZI_Settings_Debug", false);

        public Action<bool> OnStartingOrStoppingChanged;
        private bool isStartingOrStopping;
        private bool isStartingOrStoppingChanged;

        // Disable hybrid mode from now on.....2023.08.10
        // This env var is for INTERNAL USER only.
        private const string ENV_ENABLE_HYBRID_MODE = "ML_ZI_ENABLE_HYBRID";
        private static readonly bool disableHybridMode = !"1".Equals(System.Environment.GetEnvironmentVariable(ENV_ENABLE_HYBRID_MODE));
        // Global switch on whether to disable hybrid mode.
        // Note the target dropdown list in Target View is determined by enum "TargetViewState.SelectableTargets" which
        // cannot be controlled by this switch on the fly.
        public static bool IsHybridDisabled()
        {
            return disableHybridMode;
        }

        public bool IsStartingOrStopping
        {
            get => isStartingOrStopping;
            set
            {
                if (isStartingOrStopping == value)
                    return;
                isStartingOrStoppingChanged = true; // set this flag to remember to invoke the OnStartingOrStoppingChanged, but on the UI thread
                isStartingOrStopping = value;
            }
        }

        public Action<SessionTargetMode> OnTargetModeChanged;
        public Action<bool> OnSessionConnectedChanged;
        private bool isConnected;

        public bool IsConnected
        {
            get => isConnected;
            private set
            {
                if (isConnected != value)
                {
                    isConnected = value;
                    OnSessionConnectedChanged?.Invoke(isConnected);
                }
            }
        }

        public Action<bool> OnServerRunningChanged;

        private bool isServerRunning = false;
        public bool IsServerRunning {
            get => isServerRunning;
            private set
            {
                if (value != isServerRunning)
                {
                    isServerRunning = value;
                    OnServerRunningChanged?.Invoke(isServerRunning);
                }
            }
        }

        private SessionTargetMode TargetMode
        {
            get => targetMode;

            set
            {
                if (targetMode == value)
                    return;
                targetMode = value;
                OnTargetModeChanged?.Invoke(targetMode);
            }
        }

        private SessionTargetMode targetMode;
        private DateTime reconnectedTimestamp;

        private ZIBridge()
        {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload += OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload += OnAfterAssemblyReload;
#endif

            EditorApplication.update += OnEditorApplicationUpdate;

            InitSession();
        }

        ~ZIBridge()
        {
#if UNITY_EDITOR
            AssemblyReloadEvents.beforeAssemblyReload -= OnBeforeAssemblyReload;
            AssemblyReloadEvents.afterAssemblyReload -= OnAfterAssemblyReload;
#endif

            EditorApplication.update -= OnEditorApplicationUpdate;
        }

        private void OnEditorApplicationUpdate()
        {
            UpdatePlugin();
            MonitorUserMessageEvents();
            if (isStartingOrStoppingChanged)
            {
                isStartingOrStoppingChanged = false;
                OnStartingOrStoppingChanged?.Invoke(isStartingOrStopping);  // invoke this from the UI thread
            }
            if (IsStartingOrStopping)
                return;

            TakeSessionChanges();
            UpdateSDKPaths();
            UpdateSessionSaveStatus();
            CheckModulesChanges();
        }

        private void CheckModulesChanges()
        {
            if (!IsConnected)
            {
                return;
            }

            Peripheral.CheckChanges();
            SceneGraph.CheckChanges();
            VirtualRoom.CheckChanges();
            EyeTrackingHandle.CheckChanges();
            HeadTrackingHandle.CheckChanges();
            LeftHandHandle.CheckChanges();
            RightHandHandle.CheckChanges();
            ConfigurationSettings.CheckChanges();
            DeviceConfiguration.CheckChanges();
            InputController.CheckChanges();
            ImageTracking.CheckChanges();
            Permissions.CheckChanges();
            SystemEvents.CheckChanges();
        }

        private void TakeSessionChanges()
        {
            session.SetHandle(GetSessionHandleFromPlugin());
            var serverRunning = session.Server.GetRunningResult();
            IsServerRunning = serverRunning == Result.Ok;
            if (!IsServerRunning)
            {
                //If previously connected, disconnect as server has stopped
                if (IsConnected)
                {
                    TryDisconnectSession();
                }
                return;
            }

            if (IsConnected)
            {
                var targetMode = TargetMode;
                try
                {
                    var modeAndExact = session.DetectTargetMode();
                    targetMode = modeAndExact.first;
                }
                catch (ResultIsErrorException)
                {
                    // ignore
                    targetMode = SessionTargetMode.Unknown;
                }
                // Pause check due to unity instability: https://magicleap.atlassian.net/browse/REM-5950
                if ((targetMode != TargetMode || EditorApplication.isPaused) && IsConnected)
                {
                    TryDisconnectSession();
                    return;
                }
            }

            // Pause check due to unity instability: https://magicleap.atlassian.net/browse/REM-5950
            if (IsConnected || EditorApplication.isPaused || DateTime.UtcNow.Subtract(reconnectedTimestamp).TotalSeconds < 2f)
            {
                return;
            }

            // try to connect 
            ReconnectSession();
        }
    }
}
