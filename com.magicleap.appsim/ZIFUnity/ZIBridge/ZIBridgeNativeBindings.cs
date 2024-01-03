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
using System.Runtime.InteropServices;
using ml.zi;

namespace MagicLeap.ZI
{
    internal sealed partial class ZIBridge
    {
        private delegate bool IsAsyncCancellingFunc();

        private delegate void PostAsyncEventFunc(AsyncEventType type, [In] [MarshalAs(UnmanagedType.LPStr)] string message, float percentDone,
                                                 EventUserMessageType userMessageType);

        public IntPtr RenderEventPtr => PluginUnityRenderEvent();

        [DllImport("zifUnity", EntryPoint = "SetColorSpaceLinear")]
        private static extern void ConfigurePluginForLinearColorSpace(bool isLinear);

        [DllImport("zifUnity", EntryPoint = "GetSessionHandle")]
        private static extern uint GetSessionHandleFromPlugin();
        
        [DllImport("zifUnity", EntryPoint = "GetRenderEventFunc")]
        private static extern IntPtr PluginUnityRenderEvent();

        [DllImport("zifUnity")]
        private static extern bool ReconnectZISession(PostAsyncEventFunc postEvent, IsAsyncCancellingFunc canceling, out SessionTargetMode sessionMode);

        [DllImport("zifUnity", EntryPoint = "ResizeViewport")]
        private static extern void ResizePluginRendererViewport(int eventID, int width, int height, float dpiScale);

        [DllImport("zifUnity", EntryPoint = "SetDeviceViewRendererSettingValue")]
        private static extern void SetDeviceViewRenderModeValue(RendererModeSetting setting, float value);

        [DllImport("zifUnity", EntryPoint = "SetDeviceViewRendererTwoEyedModeEnabled")]
        private static extern void SetDeviceViewRenderTwoEyedMode(bool enabled);

        [DllImport("zifUnity")]
        private static extern bool StartZISession(SessionTargetMode sessionMode, PostAsyncEventFunc postEvent, IsAsyncCancellingFunc cancelling);

        [DllImport("zifUnity")]
        private static extern bool StopZISession(PostAsyncEventFunc postEvent, IsAsyncCancellingFunc canceling);

        [DllImport("zifUnity", EntryPoint = "SetDeviceViewRendererMode")]
        private static extern void UpdateDeviceRendererToMode(RendererMode rendererMode);

        public void EnableTwoEyeModeForDeviceView(bool useTwoEyedMode)
        {
            SetDeviceViewRenderTwoEyedMode(useTwoEyedMode);
        }

        public void ResizeRendererViewport(PeripheralInputSource source, int width, int height, float scale)
        {
            ResizePluginRendererViewport((int) source, width, height, scale);
        }

        public void SetDeviceViewRenderMode(RendererMode rendererMode)
        {
            UpdateDeviceRendererToMode(rendererMode);
        }

        public void SetDeviceViewRenderSettingValue(RendererModeSetting modeSetting, float newValue)
        {
            SetDeviceViewRenderModeValue(modeSetting, newValue);
        }

        public void SetUsingLinearColorSpace(bool useLinearSetting)
        {
            ConfigurePluginForLinearColorSpace(useLinearSetting);
        }
    }
}
