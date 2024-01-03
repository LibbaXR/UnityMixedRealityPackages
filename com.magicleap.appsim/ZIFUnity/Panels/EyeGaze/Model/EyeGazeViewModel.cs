// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;

namespace MagicLeap.ZI
{
    internal partial class EyeGazeViewModel : ViewModel
    {
        private ZIBridge.ModuleWrapper<ConfigurationSettings, ConfigurationSettingsChanges> ConfigurationSettings =>
            Bridge.ConfigurationSettings;

        private ZIBridge.ModuleWrapper<EyeTracking, EyeTrackingChanges> EyeTracking =>
            ZIBridge.Instance.EyeTrackingHandle;

        public override void Initialize()
        {
            EyeTracking.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            ConfigurationSettings.OnHandleConnectionChanged += SessionConnectionStatusChanged;

            base.Initialize();

            ConfigurationSettings.OnTakeChanges += OnConfigurationSettingsChanged;
            Bridge.EyeTrackingHandle.OnTakeChanges += EyeTrackingChanged;

            ZIBridge.Instance.EyeTrackingHandle.StartListening(this);
        }

        public override void UnInitialize()
        {
            EyeTracking.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            ConfigurationSettings.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            
            base.UnInitialize();
            
            ConfigurationSettings.OnTakeChanges -= OnConfigurationSettingsChanged;
            Bridge.EyeTrackingHandle.OnTakeChanges -= EyeTrackingChanged;
            
            ZIBridge.Instance.EyeTrackingHandle.StopListening(this);
        }

        protected override bool AreRequiredModulesConnected()
        {
            return Bridge.IsConnected && EyeTracking.IsHandleConnected &&
                   ConfigurationSettings.IsHandleConnected;
        }


        private void EyeTrackingChanged(EyeTrackingChanges changes)
        {
            CheckEyeTrackingChanges(changes);
            CheckGazeRecognitionChanges(changes);
        }

        private void OnConfigurationSettingsChanged(ConfigurationSettingsChanges changes)
        {
            if (changes.HasFlag(ConfigurationSettingsChanges.ConfigurationSettingsChanged))
            {
                PupilDistanceChanged?.Invoke(GetPupilDistance());
            }
        }
    }
}
