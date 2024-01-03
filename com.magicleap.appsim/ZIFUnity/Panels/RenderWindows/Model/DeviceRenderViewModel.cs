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
    internal class DeviceRenderViewModel : RenderViewModel
    {
        public override PeripheralInputSource InputSource => PeripheralInputSource.DeviceView;

        private ZIBridge.PausingModuleWrapper<HeadTracking, HeadTrackingChanges> HeadTracking => ZIBridge.Instance.HeadTrackingHandle;
        private ZIBridge.ModuleWrapper<VirtualRoom, VirtualRoomChanges> VirtualRoom  => ZIBridge.Instance.VirtualRoom;

        public override void Initialize()
        {
            VirtualRoom.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            Peripheral.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
            HeadTracking.StartListening(this);
        }

        public override void UnInitialize()
        {
            VirtualRoom.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            Peripheral.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
            HeadTracking.StopListening(this);
        }

        public void ResetHeadpose()
        {
            if (HeadTracking.IsHandleConnected)
            {
                HeadTracking.Handle.ResetHeadpose();
            }
        }

        public void ToggleTwoEyedMode()
        {
            Settings.Instance.TwoEyeRenderingEnabled = !Settings.Instance.TwoEyeRenderingEnabled;
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && VirtualRoom.IsHandleConnected &&
                   Peripheral.IsHandleConnected;
        }
    }
}
