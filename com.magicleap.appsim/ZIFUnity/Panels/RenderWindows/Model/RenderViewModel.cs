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
using System;

namespace MagicLeap.ZI
{
    internal abstract class RenderViewModel : ViewModel
    {
        public event Action OnConnectionStatusChanged;

        public abstract PeripheralInputSource InputSource { get; }
        protected ZIBridge.ModuleWrapper<Peripheral, PeripheralChanges> Peripheral => Bridge.Peripheral;

        public void PostKeyCode(bool pressed, KeyCode key, KeyModifiers modifiers)
        {
            if (Peripheral.IsHandleConnected)
            {
               PeripheralInputEventKey evt = PeripheralInputEventKey.Alloc(pressed, key, modifiers, InputSource);
               Peripheral.Handle.PostInput(evt);
            }
        }

        public void PostMouseButton(bool pressed, MouseButton mouseButton, KeyModifiers modifierKeys)
        {
            if (Peripheral.IsHandleConnected)
            {
               PeripheralInputEventMouseButton evt = PeripheralInputEventMouseButton.Alloc(pressed, mouseButton, modifierKeys, InputSource);
               Peripheral.Handle.PostInput(evt);
            }
        }

        public void PostMouseMove(float x, float y, KeyModifiers modifierKeys)
        {
            if (Peripheral.IsHandleConnected)
            {
               PeripheralInputEventMouseMove evt = PeripheralInputEventMouseMove.Alloc(x, y, modifierKeys, InputSource);
               Peripheral.Handle.PostInput(evt);
            }
        }

        public void PostMouseScroll(float x, float y, KeyModifiers modifierKeys)
        {
            if (Peripheral.IsHandleConnected)
            {
               PeripheralInputEventMouseScroll evt = PeripheralInputEventMouseScroll.Alloc(x, y, modifierKeys, InputSource);
               Peripheral.Handle.PostInput(evt);
            }
        }

        protected override void SessionStarted()
        {
            base.SessionStarted();
            UpdateValues();
        }

        protected virtual void UpdateValues()
        {
            if (!Peripheral.IsHandleConnected)
                return;
            
            if (Peripheral.Handle.GetTargetMode() == PeripheralTargetMode.Unknown)
            {
                SetTargetMode();
            }
        }

        protected override void SessionConnectionStatusChanged(bool connectionStatus)
        {
            base.SessionConnectionStatusChanged(connectionStatus);

            OnConnectionStatusChanged?.Invoke();
        }

        private void SetTargetMode()
        {
            if (!Peripheral.IsHandleConnected)
                return;
            
            switch (InputSource)
            {
                case PeripheralInputSource.SceneView:
                   Peripheral.Handle.SetTargetMode(PeripheralTargetMode.SceneView);
                    break;
                case PeripheralInputSource.DeviceView:
                   Peripheral.Handle.SetTargetMode(PeripheralTargetMode.Headpose);
                    break;
            }
        }
    }
}
