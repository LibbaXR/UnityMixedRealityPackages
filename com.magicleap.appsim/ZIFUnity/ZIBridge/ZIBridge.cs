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

        public Action<bool> IsStartingOrStoppingChanged;
        private bool isStartingOrStopping;

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

                isStartingOrStopping = value;
                IsStartingOrStoppingChanged?.Invoke(isStartingOrStopping);
            }
        }

        public Action<SessionTargetMode> OnTargetModeChanged;
        public Action<bool> OnSessionConnectedChanged;
        private bool isConnected;

        public bool IsConnected
        {
            get => isConnected;
            set
            {
                if (isConnected == value)
                    return;

                isConnected = value;
                OnSessionConnectedChanged?.Invoke(isConnected);
            }
        }

        private bool IsServerRunning { get; set; }

        private SessionTargetMode TargetMode
        {
            get => targetMode;

            set
            {
                if (targetMode == value)
                    return;

                targetMode = value;

                OnTargetModeChanged?.Invoke(targetMode);
                connectedTimestamp = DateTime.UtcNow;
            }
        }

        private SessionTargetMode targetMode;
        private DateTime connectedTimestamp;

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
            if (IsStartingOrStopping)
                return;

            TakeSessionChanges();
            UpdateSDKPaths();
            UpdateSessionSaveStatus();
            CheckModulesChanges();
            MonitorUserMessageEvents();
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

            try
            {
                var modeAndExact = session.DetectTargetMode();
                TargetMode = modeAndExact.first;
            }
            catch (ResultIsErrorException)
            {
                // ignore
                TargetMode = SessionTargetMode.Unknown;
            }

            if (TargetMode == SessionTargetMode.Unknown && IsConnected)
            {
                TryDisconnectSession();
                return;
            }

            if (TargetMode == SessionTargetMode.Unknown)
                return;

            if (IsConnected || DateTime.UtcNow.Subtract(connectedTimestamp).TotalSeconds < 2f)
                return;

            // try to connect 
            IsConnected = ReconnectSession(out var reconnectedTargetMode);
        }
    }
}
