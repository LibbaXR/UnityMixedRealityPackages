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
    internal class HeadPoseViewController : ViewController<HeadPoseViewModel, HeadPoseViewPresenter>
    {
        private static readonly string windowName = "App Sim Head Pose";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Head Pose #F9", false, MenuItemPriority_HeadPose)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Head Pose", isValidateFunction: false, priority: MenuItemPriority_HeadPose)]
#endif
        public static void ShowWindow()
        {
            GetWindow<HeadPoseViewController>(windowName);
        }

        protected override void Update()
        {
            base.Update();

            Presenter.RefreshMapEventsButtons();
        }

        protected void OnDisable()
        {
            Presenter.OnResetTransformClicked -= Model.ResetHeadPose;
            Presenter.OnCurrentPositionChanged -= Model.SetPosition;
            Presenter.OnCurrentOrientationChanged -= Model.SetOrientation;
            Presenter.OnCurrentConfidenceChanged -= Model.SetConfidence;
            Presenter.OnCurrentModeChanged -= Model.SetHeadTrackingMode;
            Presenter.OnCurrentErrorChanged -= Model.SetError;
            Presenter.OnTriggerHeadTrackingEvent -= Model.SetMapEvents;
            Presenter.OnAlignDeviceToSceneView -= Model.AlignDeviceToSceneView;
            Presenter.OnAlignSceneToDeviceView -= Model.AlignSceneViewToDevice;
            Presenter.OnDisable();

            Model.OnSessionStarted -= OnSessionStart;
            Model.OnSessionStopped -= OnSessionStopped;
            Model.OnPositionChanged -= Presenter.SetPosition;
            Model.OnOrientationChanged -= Presenter.SetOrientation;
            Model.ConfidenceChanged -= Presenter.SetTrackingConfidence;
            Model.HeadTrackingModeChanged -= Presenter.SetHeadTrackingMode;
            Model.ErrorChanged -= Presenter.SetHeadTrackingError;
            Model.MapEventsChanged -= Presenter.SetTrackingMapEvents;
            Model.UnInitialize();
        }

        protected override void Initialize()
        {
            Presenter.OnEnable(rootVisualElement);
            Presenter.OnResetTransformClicked += Model.ResetHeadPose;
            Presenter.OnCurrentPositionChanged += Model.SetPosition;
            Presenter.OnCurrentOrientationChanged += Model.SetOrientation;
            Presenter.OnCurrentConfidenceChanged += Model.SetConfidence;
            Presenter.OnCurrentModeChanged += Model.SetHeadTrackingMode;
            Presenter.OnCurrentErrorChanged += Model.SetError;
            Presenter.OnTriggerHeadTrackingEvent += Model.SetMapEvents;
            Presenter.OnAlignDeviceToSceneView += Model.AlignDeviceToSceneView;
            Presenter.OnAlignSceneToDeviceView += Model.AlignSceneViewToDevice;

            Model.OnSessionStarted += OnSessionStart;
            Model.OnSessionStopped += OnSessionStopped;
            Model.OnPositionChanged += Presenter.SetPosition;
            Model.OnOrientationChanged += Presenter.SetOrientation;
            Model.ConfidenceChanged += Presenter.SetTrackingConfidence;
            Model.HeadTrackingModeChanged += Presenter.SetHeadTrackingMode;
            Model.ErrorChanged += Presenter.SetHeadTrackingError;
            Model.MapEventsChanged += Presenter.SetTrackingMapEvents;
            
            base.Initialize();
            Presenter.SetInteractable(Model.IsSessionRunning);
            Presenter.SetHybridAndDeviceVisualElements(!Model.IsDeviceMode);
        }

        private void OnSessionStart()
        {
            Presenter.SetInteractable(true);
            Presenter.SetHybridAndDeviceVisualElements(!Model.IsDeviceMode);

            Presenter.SetPosition(Model.GetPosition());
            Presenter.SetOrientation(Model.GetOrientation());
            Presenter.SetTrackingConfidence(Model.GetConfidence());
            Presenter.SetHeadTrackingMode(Model.GetHeadTrackingMode());
            Presenter.SetHeadTrackingError(Model.GetError());
            Presenter.SetTrackingMapEvents(Model.GetMapEvents());
        }

        private void OnSessionStopped()
        {
            Presenter.SetInteractable(false);
        }
    }
}
