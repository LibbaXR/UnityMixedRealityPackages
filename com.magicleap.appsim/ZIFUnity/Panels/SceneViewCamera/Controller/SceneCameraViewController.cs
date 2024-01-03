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
    internal class SceneCameraViewController : ViewController<SceneCameraViewModel, SceneCameraViewPresenter>
    {
        private static readonly string windowName = "App Sim Scene Camera View";

        private void OnDisable()
        {
            Presenter.OnResetTransformClicked -= Model.ResetSceneViewCamera;
            Presenter.OnCurrentOrientationChanged -= Model.SetOrientation;
            Presenter.OnCurrentPositionChanged -= Model.SetPosition;
            Presenter.OnAlignDeviceToSceneView -= Model.AlignDeviceToSceneView;
            Presenter.OnAlignSceneToDeviceView -= Model.AlignSceneViewToDevice;
            Presenter.OnDisable();

            Model.OnOrientationChanged -= Presenter.SetOrientation;
            Model.OnPositionChanged -= Presenter.SetPosition;
            Model.OnSessionStarted -= OnSessionStart;
            Model.OnSessionStopped -= OnSessionStopped;
            Model.UnInitialize();
        }

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Scene Camera", false, MenuItemPriority_SceneCamera)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Scene Camera", isValidateFunction: false, priority: MenuItemPriority_SceneCamera)]
#endif
        public static void ShowWindow()
        {
            GetWindow<SceneCameraViewController>(windowName);
        }

        protected override void Initialize()
        {
            Presenter.OnEnable(rootVisualElement);
            Presenter.OnResetTransformClicked += Model.ResetSceneViewCamera;
            Presenter.OnCurrentPositionChanged += Model.SetPosition;
            Presenter.OnCurrentOrientationChanged += Model.SetOrientation;
            Presenter.OnAlignDeviceToSceneView += Model.AlignDeviceToSceneView;
            Presenter.OnAlignSceneToDeviceView += Model.AlignSceneViewToDevice;

            Model.OnSessionStarted += OnSessionStart;
            Model.OnSessionStopped += OnSessionStopped;
            Model.OnPositionChanged += Presenter.SetPosition;
            Model.OnOrientationChanged += Presenter.SetOrientation;
            base.Initialize();
            Presenter.SetInteractable(Model.IsSessionRunning);
        }

        private void OnSessionStart()
        {
            Presenter.SetInteractable(true);
            Presenter.SetPosition(Model.GetPosition());
            Presenter.SetOrientation(Model.GetOrientation());
        }

        private void OnSessionStopped()
        {
            Presenter.SetInteractable(false);
        }
    }
}
