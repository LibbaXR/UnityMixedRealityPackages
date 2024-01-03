// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEditor;

namespace MagicLeap.ZI
{
    internal class DeviceViewController : RenderViewController<DeviceRenderViewModel, DeviceRenderViewPresenter, DeviceRenderViewState>
    {
        private static readonly string windowName = "App Sim Device View";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Device View #F2", false, MenuItemPriority_DeviceView)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Device View", isValidateFunction: false, priority: MenuItemPriority_DeviceView)]
#endif
        private static void ShowWindow()
        {
            GetWindow<DeviceViewController>(windowName);
        }

        protected override void Initialize()
        {
            base.Initialize();
            
            Presenter.TwoEyedModeToggleButtonHandler += HandleTwoEyedToggleButton;
            Presenter.ResetHeadposeButtonHandler += HandleResetHeadposeButton;
            
            Model.Initialize();
        }
        
        protected override void OnDisable()
        {
            base.OnDisable();
            
            Presenter.TwoEyedModeToggleButtonHandler -= HandleTwoEyedToggleButton;
            Presenter.ResetHeadposeButtonHandler -= HandleResetHeadposeButton;
            
            Model.UnInitialize();
        }
        
        private void HandleResetHeadposeButton()
        {
            Model.ResetHeadpose();
        }

        private void HandleTwoEyedToggleButton()
        {
            Model.ToggleTwoEyedMode();
        }
    }
}
