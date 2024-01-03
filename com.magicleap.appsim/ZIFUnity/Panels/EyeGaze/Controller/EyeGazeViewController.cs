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
    internal class EyeGazeViewController : ViewController<EyeGazeViewModel, EyeGazeViewPresenter>
    {
        private static readonly string windowName = "App Sim Eye Gaze";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Eye Gaze #F5", false, MenuItemPriority_EyeGaze)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Eye Gaze", isValidateFunction: false, priority: MenuItemPriority_EyeGaze)]
#endif
        public static void ShowWindow()
        {
            GetWindow<EyeGazeViewController>(windowName);
        }

        private void OnDisable()
        {
            UnregisterCallbacksFromPresenter();
            UnregisterCallbacksFromModel();

            Presenter.OnDisable();
            Model.UnInitialize();
        }

        protected override void Initialize()
        {
            RegisterCallbacksFromPresenter();
            RegisterCallbacksFromModel();
            Presenter.OnEnable(rootVisualElement);
            
            base.Initialize();
            Presenter.SetPanelActive(Model.IsSessionRunning);
            if (Model.IsHybridMode || Model.IsDeviceMode)
            {
                Presenter.EyeTrackingSubView.DisableHybridFields();
            }
        }

        private void OnSessionConnected()
        {
            Presenter.SetPanelActive(true);
            if (Model.IsHybridMode || Model.IsDeviceMode)
            {
                Presenter.EyeTrackingSubView.DisableHybridFields();
            }

            Presenter.EyeTrackingSubView.SetCalibrationStatus(Model.GetCalibrationStatus());
            Presenter.EyeTrackingSubView.SetErrorState(Model.GetErrorState());
            Presenter.EyeTrackingSubView.SetFixationConfidence(Model.GetFixationConfidence());
            Presenter.EyeTrackingSubView.SetPupilDistance(Model.GetPupilDistance());
            Presenter.EyeTrackingSubView.SetFollowHeadPose(Model.GetFollowHeadPose());
            Presenter.EyeTrackingSubView.SetLeftPupilSize(Model.GetLeftPupilSize());
            Presenter.EyeTrackingSubView.SetRightEyeConfidence(Model.GetRightEyeConfidence());
            Presenter.EyeTrackingSubView.SetRightPupilSize(Model.GetRightPupilSize());
            Presenter.EyeTrackingSubView.SetFixationPosition(Model.GetFixationPosition());
            Presenter.EyeTrackingSubView.SetLeftEyeBlink(Model.GetLeftEyeBlink());
            Presenter.EyeTrackingSubView.SetRightEyeBlink(Model.GetRightEyeBlink());
            Presenter.EyeTrackingSubView.SetRightEyePosition(Model.GetRightEyePosition());
            Presenter.EyeTrackingSubView.SetLeftEyePosition(Model.GetLeftEyePosition());
            Presenter.EyeTrackingSubView.SetLeftEyeConfidence(Model.GetLeftEyeConfidence());
            
            Presenter.GazeRecognitionSubView.SetGazeLeftPupilPosition(Model.GetGazeLeftPupilPosition());
            Presenter.GazeRecognitionSubView.SetGazeRightPupilPosition(Model.GetGazeRightPupilPosition());
            Presenter.GazeRecognitionSubView.SetGazeAmplitude(Model.GetGazeAmplitude());
            Presenter.GazeRecognitionSubView.SetGazeBehavior(Model.GetGazeBehavior());
            Presenter.GazeRecognitionSubView.SetGazeDirection(Model.GetGazeDirection());
            Presenter.GazeRecognitionSubView.SetGazeDuration(Model.GetGazeDuration());
            Presenter.GazeRecognitionSubView.SetGazeError(Model.GetGazeError());
            Presenter.GazeRecognitionSubView.SetGazeVelocity(Model.GetGazeVelocity());
        }

        private void OnSessionDisconnected()
        {
            Presenter.SetPanelActive(false);
        }

        private void RegisterCallbacksFromModel()
        {
            Model.OnSessionStarted += OnSessionConnected;
            Model.OnSessionStopped += OnSessionDisconnected;

            Model.FollowHeadPoseChanged += Presenter.EyeTrackingSubView.SetFollowHeadPose;
            Model.FixationPositionChanged += Presenter.EyeTrackingSubView.SetFixationPosition;
            Model.LeftEyePositionChanged += Presenter.EyeTrackingSubView.SetLeftEyePosition;
            Model.RightEyePositionChanged += Presenter.EyeTrackingSubView.SetRightEyePosition;
            Model.LeftEyeBlinkChanged += Presenter.EyeTrackingSubView.SetLeftEyeBlink;
            Model.RightEyeBlinkChanged += Presenter.EyeTrackingSubView.SetRightEyeBlink;
            Model.PupilDistanceChanged += Presenter.EyeTrackingSubView.SetPupilDistance;
            Model.PupilLeftSizeChanged += Presenter.EyeTrackingSubView.SetLeftPupilSize;
            Model.PupilRightSizeChanged += Presenter.EyeTrackingSubView.SetRightPupilSize;
            Model.FixationConfidenceChanged += Presenter.EyeTrackingSubView.SetFixationConfidence;
            Model.LeftEyeConfidenceChanged += Presenter.EyeTrackingSubView.SetLeftEyeConfidence;
            Model.RightEyeConfidenceChanged += Presenter.EyeTrackingSubView.SetRightEyeConfidence;
            Model.ErrorStateChanged += Presenter.EyeTrackingSubView.SetErrorState;
            Model.CalibrationStatusChanged += Presenter.EyeTrackingSubView.SetCalibrationStatus;
            Model.ActiveOnDeviceChanged += Presenter.EyeTrackingSubView.ActiveOnDeviceChanged;

            Model.GazeOnSetChanged += Presenter.GazeRecognitionSubView.SetGazeOnset;
            Model.GazeDurationChanged += Presenter.GazeRecognitionSubView.SetGazeDuration;
            Model.GazeVelocityChanged += Presenter.GazeRecognitionSubView.SetGazeVelocity;
            Model.GazeAmplitudeChanged += Presenter.GazeRecognitionSubView.SetGazeAmplitude;
            Model.GazeDirectionChanged += Presenter.GazeRecognitionSubView.SetGazeDirection;
            Model.GazeErrorChanged += Presenter.GazeRecognitionSubView.SetGazeError;
            Model.GazeBehaviorChanged += Presenter.GazeRecognitionSubView.SetGazeBehavior;
            Model.GazeLeftPupilPositionChanged += Presenter.GazeRecognitionSubView.SetGazeLeftPupilPosition;
            Model.GazeRightPupilPositionChanged += Presenter.GazeRecognitionSubView.SetGazeRightPupilPosition;
        }

        private void RegisterCallbacksFromPresenter()
        {
            Presenter.OnResetTransformClicked += Model.ResetEyeFixation;
            Presenter.EyeTrackingSubView.FollowHeadPoseChanged += Model.SetFollowHeadPose;
            Presenter.EyeTrackingSubView.FixationPositionChanged += Model.SetFixationPosition;
            Presenter.EyeTrackingSubView.LeftEyePositionChanged += Model.SetLeftEyePosition;
            Presenter.EyeTrackingSubView.RightEyePositionChanged += Model.SetRightEyePosition;
            Presenter.EyeTrackingSubView.LeftEyeBlinkChanged += Model.SetLeftEyeBlink;
            Presenter.EyeTrackingSubView.RightEyeBlinkChanged += Model.SetRightEyeBlink;
            Presenter.EyeTrackingSubView.PupilDistanceChanged += Model.SetPupilDistance;
            Presenter.EyeTrackingSubView.RightPupilSizeChanged += Model.SetRightPupilSize;
            Presenter.EyeTrackingSubView.LeftPupilSizeChanged += Model.SetLeftPupilSize;

            Presenter.EyeTrackingSubView.FixationConfidenceChanged += Model.SetFixationConfidence;
            Presenter.EyeTrackingSubView.LeftEyeConfidenceChanged += Model.SetLeftEyeConfidence;
            Presenter.EyeTrackingSubView.RightEyeConfidenceChanged += Model.SetRightEyeConfidence;

            Presenter.EyeTrackingSubView.ErrorStateChanged += Model.SetErrorState;
            Presenter.EyeTrackingSubView.CalibrationStatusChanged += Model.SetCalibrationStatus;

            Presenter.GazeRecognitionSubView.GazeDurationChanged += Model.SetGazeDuration;
            Presenter.GazeRecognitionSubView.GazeVelocityChanged += Model.SetGazeVelocity;
            Presenter.GazeRecognitionSubView.GazeAmplitudeChanged += Model.SetGazeAmplitude;
            Presenter.GazeRecognitionSubView.GazeBehaviorChanged += Model.SetGazeBehavior;
            Presenter.GazeRecognitionSubView.GazeDirectionChanged += Model.SetGazeDirection;
            Presenter.GazeRecognitionSubView.GazeOnSetChanged += Model.SetGazeOnset;
            Presenter.GazeRecognitionSubView.GazeErrorChanged += Model.SetGazeError;
            Presenter.GazeRecognitionSubView.GazeLeftPupilPositionChanged += Model.SetGazeLeftPupilPosition;
            Presenter.GazeRecognitionSubView.GazeRightPupilPositionChanged += Model.SetGazeRightPupilPosition;
        }

        private void UnregisterCallbacksFromModel()
        {
            Model.OnSessionStarted -= OnSessionConnected;
            Model.OnSessionStopped -= OnSessionDisconnected;

            Model.FollowHeadPoseChanged -= Presenter.EyeTrackingSubView.SetFollowHeadPose;
            Model.FixationPositionChanged -= Presenter.EyeTrackingSubView.SetFixationPosition;
            Model.LeftEyePositionChanged -= Presenter.EyeTrackingSubView.SetLeftEyePosition;
            Model.RightEyePositionChanged -= Presenter.EyeTrackingSubView.SetRightEyePosition;
            Model.LeftEyeBlinkChanged -= Presenter.EyeTrackingSubView.SetLeftEyeBlink;
            Model.RightEyeBlinkChanged -= Presenter.EyeTrackingSubView.SetRightEyeBlink;
            Model.PupilDistanceChanged -= Presenter.EyeTrackingSubView.SetPupilDistance;
            Model.PupilLeftSizeChanged -= Presenter.EyeTrackingSubView.SetLeftPupilSize;
            Model.PupilRightSizeChanged -= Presenter.EyeTrackingSubView.SetRightPupilSize;
            Model.FixationConfidenceChanged -= Presenter.EyeTrackingSubView.SetFixationConfidence;
            Model.LeftEyeConfidenceChanged -= Presenter.EyeTrackingSubView.SetLeftEyeConfidence;
            Model.RightEyeConfidenceChanged -= Presenter.EyeTrackingSubView.SetRightEyeConfidence;
            Model.ErrorStateChanged -= Presenter.EyeTrackingSubView.SetErrorState;
            Model.CalibrationStatusChanged -= Presenter.EyeTrackingSubView.SetCalibrationStatus;
            Model.ActiveOnDeviceChanged -= Presenter.EyeTrackingSubView.ActiveOnDeviceChanged;
            
            
            Model.GazeOnSetChanged -= Presenter.GazeRecognitionSubView.SetGazeOnset;
            Model.GazeDurationChanged -= Presenter.GazeRecognitionSubView.SetGazeDuration;
            Model.GazeVelocityChanged -= Presenter.GazeRecognitionSubView.SetGazeVelocity;
            Model.GazeAmplitudeChanged -= Presenter.GazeRecognitionSubView.SetGazeAmplitude;
            Model.GazeDirectionChanged -= Presenter.GazeRecognitionSubView.SetGazeDirection;
            Model.GazeErrorChanged -= Presenter.GazeRecognitionSubView.SetGazeError;
            Model.GazeBehaviorChanged -= Presenter.GazeRecognitionSubView.SetGazeBehavior;
            Model.GazeLeftPupilPositionChanged -= Presenter.GazeRecognitionSubView.SetGazeLeftPupilPosition;
            Model.GazeRightPupilPositionChanged -= Presenter.GazeRecognitionSubView.SetGazeRightPupilPosition;
        }

        private void UnregisterCallbacksFromPresenter()
        {
            Presenter.OnResetTransformClicked -= Model.ResetEyeFixation;
            Presenter.EyeTrackingSubView.FollowHeadPoseChanged -= Model.SetFollowHeadPose;
            Presenter.EyeTrackingSubView.FixationPositionChanged -= Model.SetFixationPosition;
            Presenter.EyeTrackingSubView.LeftEyePositionChanged -= Model.SetLeftEyePosition;
            Presenter.EyeTrackingSubView.RightEyePositionChanged -= Model.SetRightEyePosition;
            Presenter.EyeTrackingSubView.LeftEyeBlinkChanged -= Model.SetLeftEyeBlink;
            Presenter.EyeTrackingSubView.RightEyeBlinkChanged -= Model.SetRightEyeBlink;
            Presenter.EyeTrackingSubView.PupilDistanceChanged -= Model.SetPupilDistance;
            Presenter.EyeTrackingSubView.RightPupilSizeChanged -= Model.SetRightPupilSize;
            Presenter.EyeTrackingSubView.LeftPupilSizeChanged -= Model.SetLeftPupilSize;

            Presenter.EyeTrackingSubView.FixationConfidenceChanged -= Model.SetFixationConfidence;
            Presenter.EyeTrackingSubView.LeftEyeConfidenceChanged -= Model.SetLeftEyeConfidence;
            Presenter.EyeTrackingSubView.RightEyeConfidenceChanged -= Model.SetRightEyeConfidence;

            Presenter.EyeTrackingSubView.ErrorStateChanged -= Model.SetErrorState;
            Presenter.EyeTrackingSubView.CalibrationStatusChanged -= Model.SetCalibrationStatus;
            
            Presenter.GazeRecognitionSubView.GazeDurationChanged -= Model.SetGazeDuration;
            Presenter.GazeRecognitionSubView.GazeVelocityChanged -= Model.SetGazeVelocity;
            Presenter.GazeRecognitionSubView.GazeAmplitudeChanged -= Model.SetGazeAmplitude;
            Presenter.GazeRecognitionSubView.GazeBehaviorChanged -= Model.SetGazeBehavior;
            Presenter.GazeRecognitionSubView.GazeDirectionChanged -= Model.SetGazeDirection;
            Presenter.GazeRecognitionSubView.GazeOnSetChanged -= Model.SetGazeOnset;
            Presenter.GazeRecognitionSubView.GazeErrorChanged -= Model.SetGazeError;
            Presenter.GazeRecognitionSubView.GazeLeftPupilPositionChanged -= Model.SetGazeLeftPupilPosition;
            Presenter.GazeRecognitionSubView.GazeRightPupilPositionChanged -= Model.SetGazeRightPupilPosition;
        }
    }
}
