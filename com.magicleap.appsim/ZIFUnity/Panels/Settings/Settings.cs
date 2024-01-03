// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2019-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.IO;
using ml.zi;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace MagicLeap.ZI
{
#if UNITY_EDITOR
    internal class Settings
    {
        public enum DirtySessionState
        {
            Prompt,
            SaveSession,
            DiscardChanges
        }

        public enum MovementMode
        {
            Fly,
            Pivot
        }

        public delegate void RendererSettingsChange(PeripheralInputSource source);

        public event RendererSettingsChange OnRendererSettingsChanged;

        public static readonly string Key_Debug = "ZI_Settings_Debug";
        public static readonly string Key_DefaultSessionPath = "ZI_Settings_DefaultSessionPath";
        public static readonly string Key_HARDWARE_ACCELERATION = "ZIPreferences.KEY_ZI_HARDWARE_ACCELERATION";
        public static readonly string Key_LogLevel = "ZI_Settings_LogLevel";
        public static readonly string Key_LogPath = "ZI_Settings_LogPath";
        public static readonly string Key_PoseDriverEnabled = "ZI_Settings_PoseDriver_Enabled";
        public static readonly string Key_PoseDriverMovement = "ZI_Settings_PoseDriver_MoveMode";
        public static readonly string Key_PoseDriverMoveSpeed = "ZI_Settings_PoseDriver_MoveSpeed";
        public static readonly string Key_PoseDriverRotSpeed = "ZI_Settings_PoseDriver_RotationSpeed";
        public static readonly string Key_RenderMode = "ZI_Settings_RendererMode";
        public static readonly string Key_RenderSettingsPrefix = "ZI_Settings_Render";
        public static readonly string Key_TwoEyeMode = "ZI_Settings_TwoEyeMode";
        public static readonly string Key_DialogPrefKey = "ZI_ShowCloseSessionDialogOnQuit";
        public static readonly string Key_CloseSessionPrefKey = "ZI_CloseSessionOnQuit";
        public static readonly string Key_DirtySessionPrompt = "ZI_Dirty_Session_Close";
        public static readonly string Key_LastAndroidSdkPathLogMessage = "ZI_Android_Sdk_Path_Log";
        public static readonly string Key_LastEnvironmentPathLogMessage = "ZI_Environment_Path_Log";
        public static readonly string Key_LastVersionLogMessage = "ZI_Version_Log";

        public static Settings Instance = new();

        private static readonly string DefaultSessionPathInRuntime = "data/VirtualRooms/ExampleRooms/OfficeSmall.room";
        private Dictionary<RendererMode, RendererSettings> deviceRendererSettings;

        private LoggingSettingsBuilder zifLoggingSettings;
        private RendererSettings currentRendererSettings;
        private RendererMode deviceViewRendererMode = RendererMode.CompositeSimulator;
        private bool twoEyeRenderingEnabled;

        public string ExampleVirtualRoomsPath => Path.Combine(ZIBridge.BackendPath, "data/VirtualRooms/ExampleRooms/");

        public bool CloseZISessionOnQuit
        {
            get => EditorPrefs.GetBool(Key_DialogPrefKey, true);
            set => EditorPrefs.SetBool(Key_DialogPrefKey, value);
        }
        
        public bool ShowCloseDialog
        {
            get => EditorPrefs.GetBool(Key_CloseSessionPrefKey, true);
            set => EditorPrefs.SetBool(Key_CloseSessionPrefKey, value);
        }

        /// <summary>
        /// VirtualRooms: Use the user's custom default room path value persisted in EditorPrefs if it exists
        /// otherwise use the example room we have chosen that ships with ZI
        /// </summary>
        public string DefaultSessionPath
        {
            get => EditorPrefs.GetString(Key_DefaultSessionPath, DefaultSessionPathInRuntime);
            set => EditorPrefs.SetString(Key_DefaultSessionPath, value);
        }

        public RendererMode DeviceViewRendererMode
        {
            get => deviceViewRendererMode;
            set
            {
                if (value != deviceViewRendererMode)
                {
                    if (value != RendererMode.Unknown)
                    {
                        deviceViewRendererMode = value;
                        if (!deviceRendererSettings.ContainsKey(value))
                        {
                            Debug.LogError("Failed to set Magic Leap App Simulator Device View Renderer Mode");
                        }
                        else
                        {
                            currentRendererSettings = GetRendererSettingsForMode(value);
                            ZIBridge.Instance.SetDeviceViewRenderMode(value);
                            OnRendererSettingsChanged?.Invoke(PeripheralInputSource.DeviceView);
                        }
                    }
                }
            }
        }

        public DirtySessionState DirtySessionPrompt
        {
            get => (DirtySessionState) EditorPrefs.GetInt(Key_DirtySessionPrompt, 0);
            set
            {
                var exists = Enum.IsDefined(typeof(DirtySessionState), value);
                if (!exists)
                {
                    Debug.LogError("Failed to set Save Dirty Session Prompt. Setting to default value.");
                    value = DirtySessionState.Prompt;
                }

                EditorPrefs.SetInt(Key_DirtySessionPrompt, (int) value);
            }
        }

        public bool EnableDebugLogging { get; set; }

        public MovementMode GameViewMovementMode { get; set; }

        public float GameViewMoveSpeed { get; set; }

        public bool GameViewPoseDriverEnabled { get; set; }

        public float GameViewRotationSpeed { get; set; } = 0.1f;

        public bool HardwareAcceleration
        {
            get => EditorPrefs.GetBool(Key_HARDWARE_ACCELERATION, true);
            set => EditorPrefs.SetBool(Key_HARDWARE_ACCELERATION, value);
        }

        public LoggingLevel LoggingLevel
        {
            get => zifLoggingSettings.GetLevel();
            set
            {
                if (value != zifLoggingSettings.GetLevel())
                {
                    zifLoggingSettings.SetLevel(value);
                    Logging.ApplySettings(zifLoggingSettings);
                }
            }
        }

        public string ModelsPath
        {
            get
            {
                if (!EditorPrefs.HasKey("modelsPath"))
                {
                    return Application.dataPath;
                }

                string path = EditorPrefs.GetString("modelsPath");
                return Directory.Exists(path) ? path : Application.dataPath;
            }
            set => EditorPrefs.SetString("modelsPath", value);
        }

        public bool TwoEyeRenderingEnabled
        {
            get => twoEyeRenderingEnabled;
            set
            {
                if (value == twoEyeRenderingEnabled)
                {
                    return;
                }

                currentRendererSettings.SetTwoEyeMode(value);
                twoEyeRenderingEnabled = value;
                ZIBridge.Instance.EnableTwoEyeModeForDeviceView(value);
                OnRendererSettingsChanged?.Invoke(PeripheralInputSource.DeviceView);
            }
        }

        private Settings()
        {
            Initialize();
        }

        public void Initialize()
        {
            // initialize settings
            zifLoggingSettings = new LoggingSettingsBuilder();
            deviceRendererSettings = new Dictionary<RendererMode, RendererSettings>();

            InitializeLoggingSettings();
            InitializeDeviceViewRendererSettings();
            InitializePoseDriverSettings();
        }

        public RendererSettings GetRendererSettingsForMode(RendererMode mode)
        {
            if (deviceRendererSettings.ContainsKey(mode))
            {
                return deviceRendererSettings[mode];
            }

            return RendererSettings.AllocForMode(RendererMode.Unknown);
        }

        public float GetRenderSettingsValueForMode(RendererMode mode, RendererModeSetting setting)
        {
            if (GetRendererSettingsForMode(mode).SupportsModeSetting(setting))
            {
                if (RendererSettings.ModeGetType(setting) == RendererModeSettingType.Float)
                {
                    return GetRendererSettingsForMode(mode).ModeGetFloat(setting);
                }
            }

            return 0.0f;
        }

        private void InitializeDeviceViewRendererSettings()
        {
            // Create a RendererSettings object for each of the 4 kinds of RendererModes we want to have configurable (for Device View)
            // Each RendererSettings object will be accessible from a dictionary via the RendererMode
            foreach (RendererMode mode in Enum.GetValues(typeof(RendererMode)))
            {
                if (mode == RendererMode.VirtualRoomOnly || mode == RendererMode.Unknown)
                {
                    continue;
                }

                try
                {
                    RendererSettings rendererSettings = RendererSettings.AllocForMode(mode);
                    deviceRendererSettings.Add(mode, rendererSettings);

                    // retrieve saved EditorPrefs values for the various settings or assign defaults
                    string modeName = mode.ToString();
                    foreach (RendererModeSetting modeSetting in Enum.GetValues(typeof(RendererModeSetting)))
                    {
                        string modeSettingName = modeSetting.ToString();
                        if (deviceRendererSettings[mode].SupportsModeSetting(modeSetting))
                        {
                            if (RendererSettings.ModeGetType(modeSetting) == RendererModeSettingType.Float)
                            {
                                float savedValue = EditorPrefs.GetFloat(Key_RenderSettingsPrefix + "." + modeName + "." + modeSettingName,
                                    RendererSettings.ModeGetFloatMetadata(modeSetting)
                                                    .first); // ReturnedFloatFloatFloat.first is understood to be the "default" value
                                rendererSettings.ModeSetFloat(modeSetting, savedValue);
                                ZIBridge.Instance.SetDeviceViewRenderSettingValue(modeSetting, savedValue);
                            }
                        }
                    }
                }
                catch (Exception e)
                {
                    Debug.LogErrorFormat("Error initializing DeviceView RendererSettings for mode {0}: {1}", mode.ToString(), e);
                }
            }

            string cachedDeviceViewRendererModeName = EditorPrefs.GetString(Key_RenderMode, deviceViewRendererMode.ToString());
            if (Enum.TryParse(cachedDeviceViewRendererModeName, out RendererMode parsedRenderMode))
            {
                deviceViewRendererMode = parsedRenderMode;
                ZIBridge.Instance.SetDeviceViewRenderMode(parsedRenderMode);
            }
            else
            {
                Debug.LogWarningFormat("Unrecognized RendererMode \"{0}\". Saved {1} will be cleared.", cachedDeviceViewRendererModeName,
                    Key_RenderMode);
                EditorPrefs.DeleteKey(Key_RenderMode);
            }

            currentRendererSettings = GetRendererSettingsForMode(deviceViewRendererMode);
            twoEyeRenderingEnabled =
                EditorPrefs.GetBool(Key_TwoEyeMode, deviceRendererSettings[deviceViewRendererMode].GetTwoEyeMode());
            ZIBridge.Instance.EnableTwoEyeModeForDeviceView(twoEyeRenderingEnabled);
            OnRendererSettingsChanged?.Invoke(PeripheralInputSource.DeviceView);
        }

        private void InitializeLoggingSettings()
        {
            // Logging: Use the user's custom values persisted in EditorPrefs if it exists,
            // otherwise use default from LoggingSettingsBuilder
            Logging.GetDefaultSettings(zifLoggingSettings);

            string cachedLogLevelName = EditorPrefs.GetString(Key_LogLevel, zifLoggingSettings.Level.ToString());
            if (Enum.TryParse(cachedLogLevelName, out LoggingLevel parsedLogLevel))
            {
                zifLoggingSettings.Level = parsedLogLevel;
                Logging.ApplySettings(zifLoggingSettings);
            }
            else
            {
                Debug.LogWarningFormat("Unrecognized LoggingLevel \"{0}\". Saved {1} will be cleared.", cachedLogLevelName, Key_LogLevel);
                EditorPrefs.DeleteKey(Key_LogLevel);
            }

            EnableDebugLogging = EditorPrefs.GetBool(Key_Debug, false);
        }

        private void InitializePoseDriverSettings()
        {
            // playmode defaults
            GameViewPoseDriverEnabled = EditorPrefs.GetBool(Key_PoseDriverEnabled, true);
            string cachedViewMovementModeName = EditorPrefs.GetString(Key_PoseDriverMovement, MovementMode.Pivot.ToString());
            if (Enum.TryParse(cachedViewMovementModeName, out MovementMode parsedMovementMode))
            {
                GameViewMovementMode = parsedMovementMode;
            }
            else
            {
                Debug.LogWarningFormat("Unrecognized MovementMode \"{0}\". Saved {1} will be cleared.", cachedViewMovementModeName,
                    Key_PoseDriverMovement);
                EditorPrefs.DeleteKey(Key_PoseDriverMovement);
            }

            GameViewMoveSpeed = EditorPrefs.GetFloat(Key_PoseDriverMoveSpeed, 0.1f);
            GameViewRotationSpeed = EditorPrefs.GetFloat(Key_PoseDriverRotSpeed, 0.1f);
        }

        public void SetDefaults()
        {
            EditorPrefs.DeleteKey(Key_Debug);
            EditorPrefs.DeleteKey(Key_DefaultSessionPath);
            EditorPrefs.DeleteKey(Key_HARDWARE_ACCELERATION);
            EditorPrefs.DeleteKey(Key_LogLevel);
            EditorPrefs.DeleteKey(Key_LogPath);
            EditorPrefs.DeleteKey(Key_PoseDriverEnabled);
            EditorPrefs.DeleteKey(Key_PoseDriverMovement);
            EditorPrefs.DeleteKey(Key_PoseDriverMoveSpeed);
            EditorPrefs.DeleteKey(Key_PoseDriverRotSpeed);
            EditorPrefs.DeleteKey(Key_RenderMode);
            EditorPrefs.DeleteKey(Key_RenderSettingsPrefix);
            EditorPrefs.DeleteKey(Key_TwoEyeMode);
            EditorPrefs.DeleteKey(Key_DialogPrefKey);
            EditorPrefs.DeleteKey(Key_CloseSessionPrefKey);
            EditorPrefs.DeleteKey(Key_DirtySessionPrompt);

            Instance.Initialize();
        }
    }
#endif
}
