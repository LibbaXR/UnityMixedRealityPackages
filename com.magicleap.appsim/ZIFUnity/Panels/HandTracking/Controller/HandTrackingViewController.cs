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
using UnityEditor;

namespace MagicLeap.ZI
{
    internal class HandTrackingViewController : ViewController<HandTrackingViewModel, HandTrackingViewPresenter>
    {
        private static readonly string windowName = "App Sim Hand Tracking";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Hand Tracking #F4", false, MenuItemPriority_HandTracking)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Hand Tracking", isValidateFunction: false, priority: MenuItemPriority_HandTracking)]
#endif
        public static void ShowWindow()
        {
            GetWindow<HandTrackingViewController>(windowName);
        }

        private void OnDisable()
        {
            UnregisterCallbacksFromPresenter();
            Presenter.OnDisable();

            UnregisterCallbacksFromModel();
            Model.UnInitialize();
        }

        protected override void Initialize()
        {
            RegisterCallbacksFromModel();
            RegisterCallbacksFromPresenter();
            Presenter.OnEnable(rootVisualElement);

            base.Initialize();
            
            Presenter.SetInteractable(Model.IsSessionRunning);
            if (Model.IsSessionRunning)
            {
                Presenter.LeftHand.SetActiveOnDevice(Model.LeftHand.GetActiveOnDevice());
                Presenter.RightHand.SetActiveOnDevice(Model.RightHand.GetActiveOnDevice());
            }
        }

        private void OnSessionStarted()
        {
            Presenter.LeftHand.SetActiveOnDevice(Model.LeftHand.GetActiveOnDevice());
            Presenter.LeftHand.SetConfidence(Model.LeftHand.GetConfidence());
            Presenter.LeftHand.SetHoldingControl(Model.LeftHand.GetHoldingControl());
            Presenter.LeftHand.SelectPoseButton(Model.LeftHand.GetTrackingGesture());
            Presenter.LeftHand.SetOrientation(Model.LeftHand.GetOrientation());
            Presenter.LeftHand.SetPosition(Model.LeftHand.GetPosition());
            Presenter.LeftHand.SetFollowHeadPose(Model.LeftHand.GetFollowHeadPose());

            Presenter.RightHand.SetActiveOnDevice(Model.RightHand.GetActiveOnDevice());
            Presenter.RightHand.SetConfidence(Model.RightHand.GetConfidence());
            Presenter.RightHand.SetHoldingControl(Model.RightHand.GetHoldingControl());
            Presenter.RightHand.SelectPoseButton(Model.RightHand.GetTrackingGesture());
            Presenter.RightHand.SetOrientation(Model.RightHand.GetOrientation());
            Presenter.RightHand.SetPosition(Model.RightHand.GetPosition());
            Presenter.RightHand.SetFollowHeadPose(Model.RightHand.GetFollowHeadPose());
            
            Presenter.SetInteractable(true);
            Presenter.LeftHand.SetActiveOnDevice(Model.LeftHand.GetActiveOnDevice());
            Presenter.RightHand.SetActiveOnDevice(Model.RightHand.GetActiveOnDevice());
        }

        private void OnSessionStopped()
        {
            Presenter.SetInteractable(false);
        }
        
        private void RegisterCallbacksFromModel()
        {
            Model.OnSessionStarted += OnSessionStarted;
            Model.OnSessionStopped += OnSessionStopped;

            Model.LeftHand.TrackingKeyPoseChanged += Presenter.OnLeftHandTrackingKeyPoseChanged;
            Model.LeftHand.ConfidenceChanged += Presenter.OnLeftHandConfidenceChanged;
            Model.LeftHand.IsHoldingControlChanged += Presenter.OnLeftHandIsHoldingControlChanged;
            Model.LeftHand.FollowHeadPoseChanged += Presenter.OnLeftHandFollowHeadPoseChanged;
            Model.LeftHand.PositionChanged += Presenter.OnLeftHandPositionChanged;
            Model.LeftHand.OrientationChanged += Presenter.OnLeftHandOrientationChanged;
            Model.LeftHand.ActiveOnDeviceChanged += Presenter.LeftHand.SetActiveOnDevice;

            Model.RightHand.TrackingKeyPoseChanged += Presenter.OnRightHandTrackingKeyPoseChanged;
            Model.RightHand.ConfidenceChanged += Presenter.OnRightHandConfidenceChanged;
            Model.RightHand.IsHoldingControlChanged += Presenter.OnRightHandIsHoldingControlChanged;
            Model.RightHand.FollowHeadPoseChanged += Presenter.OnRightHandFollowHeadPoseChanged;
            Model.RightHand.PositionChanged += Presenter.OnRightHandPositionChanged;
            Model.RightHand.OrientationChanged += Presenter.OnRightHandOrientationChanged;
            Model.RightHand.ActiveOnDeviceChanged += Presenter.RightHand.SetActiveOnDevice;
        }

        private void RegisterCallbacksFromPresenter()
        {
            Presenter.OnLeftHandResetCenter += Model.LeftHand.ResetHandCenter;
            Presenter.OnRightHandResetCenter += Model.RightHand.ResetHandCenter;
            Presenter.OnLeftKeyPoseSelected += Model.LeftHand.SetTrackingKeyPose;
            Presenter.OnLeftHandConfidenceChange += Model.LeftHand.SetConfidence;
            Presenter.OnLeftIsHoldingControlChange += Model.LeftHand.SetHoldingControl;
            Presenter.OnLeftFollowHeadPoseChange += Model.LeftHand.SetFollowHeadPose;
            Presenter.OnLeftPositionChange += Model.LeftHand.SetPosition;
            Presenter.OnLeftOrientationChange += Model.LeftHand.SetOrientation;

            Presenter.OnRightKeyPoseSelected += Model.RightHand.SetTrackingKeyPose;
            Presenter.OnRightHandConfidenceChange += Model.RightHand.SetConfidence;
            Presenter.OnRightIsHoldingControlChange += Model.RightHand.SetHoldingControl;
            Presenter.OnRightFollowHeadPoseChange += Model.RightHand.SetFollowHeadPose;
            Presenter.OnRightPositionChange += Model.RightHand.SetPosition;
            Presenter.OnRightOrientationChange += Model.RightHand.SetOrientation;
        }

        private void UnregisterCallbacksFromModel()
        {
            Model.OnSessionStarted -= OnSessionStarted;
            Model.OnSessionStopped -= OnSessionStopped;

            Model.LeftHand.TrackingKeyPoseChanged -= Presenter.OnLeftHandTrackingKeyPoseChanged;
            Model.LeftHand.ConfidenceChanged -= Presenter.OnLeftHandConfidenceChanged;
            Model.LeftHand.IsHoldingControlChanged -= Presenter.OnLeftHandIsHoldingControlChanged;
            Model.LeftHand.FollowHeadPoseChanged -= Presenter.OnLeftHandFollowHeadPoseChanged;
            Model.LeftHand.PositionChanged -= Presenter.OnLeftHandPositionChanged;
            Model.LeftHand.OrientationChanged -= Presenter.OnLeftHandOrientationChanged;

            Model.RightHand.TrackingKeyPoseChanged -= Presenter.OnRightHandTrackingKeyPoseChanged;
            Model.RightHand.ConfidenceChanged -= Presenter.OnRightHandConfidenceChanged;
            Model.RightHand.IsHoldingControlChanged -= Presenter.OnRightHandIsHoldingControlChanged;
            Model.RightHand.FollowHeadPoseChanged -= Presenter.OnRightHandFollowHeadPoseChanged;
            Model.RightHand.PositionChanged -= Presenter.OnRightHandPositionChanged;
            Model.RightHand.OrientationChanged -= Presenter.OnRightHandOrientationChanged;
        }

        private void UnregisterCallbacksFromPresenter()
        {
            Presenter.OnLeftHandResetCenter -= Model.LeftHand.ResetHandCenter;
            Presenter.OnRightHandResetCenter -= Model.RightHand.ResetHandCenter;
            Presenter.OnLeftKeyPoseSelected -= Model.LeftHand.SetTrackingKeyPose;
            Presenter.OnLeftHandConfidenceChange -= Model.LeftHand.SetConfidence;
            Presenter.OnLeftIsHoldingControlChange -= Model.LeftHand.SetHoldingControl;
            Presenter.OnLeftFollowHeadPoseChange -= Model.LeftHand.SetFollowHeadPose;
            Presenter.OnLeftPositionChange -= Model.LeftHand.SetPosition;
            Presenter.OnLeftOrientationChange -= Model.LeftHand.SetOrientation;

            Presenter.OnRightKeyPoseSelected -= Model.RightHand.SetTrackingKeyPose;
            Presenter.OnRightHandConfidenceChange -= Model.RightHand.SetConfidence;
            Presenter.OnRightIsHoldingControlChange -= Model.RightHand.SetHoldingControl;
            Presenter.OnRightFollowHeadPoseChange -= Model.RightHand.SetFollowHeadPose;
            Presenter.OnRightPositionChange -= Model.RightHand.SetPosition;
            Presenter.OnRightOrientationChange -= Model.RightHand.SetOrientation;
        }
    }
}
