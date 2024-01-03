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
using ml.zi;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed partial class EyeTrackingSubView
    {
        private bool leftEyeBlink;

        private bool rightEyeBlink;

        private bool LeftEyeBlink
        {
            get => leftEyeBlink;
            set
            {
                leftEyeBlink = value;
                SetLeftEyeBlink(leftEyeBlink);
                LeftEyeBlinkChanged?.Invoke(leftEyeBlink);
            }
        }

        private bool RightEyeBlink
        {
            get => rightEyeBlink;
            set
            {
                rightEyeBlink = value;
                SetRightEyeBlink(rightEyeBlink);
                RightEyeBlinkChanged?.Invoke(rightEyeBlink);
            }
        }

        public void SetPanelActive(bool isEnabled)
        {
            followHeadPoseToggle.SetEnabled(isEnabled);
            rightEyeField.SetEnabled(isEnabled);
            leftEyeField.SetEnabled(isEnabled);
            eyeBlinkPanel.SetEnabled(isEnabled);
            leftWinkButton.SetEnabled(isEnabled);
            rightWinkButton.SetEnabled(isEnabled);

            leftWinkLabel.SetEnabled(isEnabled);
            rightWinkLabel.SetEnabled(isEnabled);

            holdToggle.SetEnabled(isEnabled);
            linkToggle.SetEnabled(isEnabled);

            fixationField.SetEnabled(isEnabled);

            errorField.SetEnabled(isEnabled);
            calibrationStatusField.SetEnabled(isEnabled);

            pupilDistanceField.SetEnabled(isEnabled);
            rightPupilSizeSlider.SetEnabled(isEnabled);
            leftPupilSizeSlider.SetEnabled(isEnabled);
            fixationSlider.SetEnabled(isEnabled);
            leftEyeSlider.SetEnabled(isEnabled);
            rightEyeSlider.SetEnabled(isEnabled);
        }

        public void RegisterUICallbacks()
        {
            followHeadPoseToggle.RegisterValueChangedCallback(FollowHeadPoseChangedCallback);

            fixationField.RegisterValueChangedCallback(FixationPositionChangedCallback);
            leftEyeField.RegisterValueChangedCallback(LeftEyePositionChangedCallback);
            rightEyeField.RegisterValueChangedCallback(RightEyePositionChangedCallback);

            leftWinkButton.RegisterCallback<PointerDownEvent>(OnLeftWinkButtonDown);
            leftWinkButton.RegisterCallback<PointerUpEvent>(OnLeftWinkButtonUp);
            leftWinkButton.RegisterCallback<ClickEvent>(OnClickedLeftWinkButton);
            leftWinkButton.RegisterCallback<PointerLeaveEvent>(OnLeaveLeftWinkButton);
            rightWinkButton.RegisterCallback<PointerDownEvent>(OnRightWinkButtonDown);
            rightWinkButton.RegisterCallback<PointerUpEvent>(OnRightWinkButtonUp);
            rightWinkButton.RegisterCallback<ClickEvent>(OnClickedRightWinkButton);
            rightWinkButton.RegisterCallback<PointerLeaveEvent>(OnLeaveRightWinkButton);

            holdToggle.RegisterValueChangedCallback(OnHoldToggle);
            linkToggle.RegisterValueChangedCallback(OnEyeBlinkLinkToggle);

            calibrationStatusField.RegisterValueChangedCallback(OnCalibrationStatusChangedCallback);
            errorField.RegisterValueChangedCallback(OnErrorTypeChanged);

            pupilDistanceField.RegisterValueChangedCallback(PupilDistanceChangedCallback);
            leftPupilSizeSlider.RegisterValueChangedCallback(LeftPupilSizeChangedCallback);
            rightPupilSizeSlider.RegisterValueChangedCallback(RightPupilSizeChangedCallback);
            fixationSlider.RegisterValueChangedCallback(FixationConfidenceChangedCallback);
            leftEyeSlider.RegisterValueChangedCallback(LeftEyeConfidenceChangedCallback);
            rightEyeSlider.RegisterValueChangedCallback(RightEyeConfidenceChangedCallback);
        }

        public void UnRegisterUICallbacks()
        {
            followHeadPoseToggle.UnregisterValueChangedCallback(FollowHeadPoseChangedCallback);

            linkToggle.UnregisterValueChangedCallback(OnEyeBlinkLinkToggle);
            holdToggle.UnregisterValueChangedCallback(OnHoldToggle);

            calibrationStatusField.UnregisterValueChangedCallback(OnCalibrationStatusChangedCallback);
            errorField.UnregisterValueChangedCallback(OnErrorTypeChanged);

            leftWinkButton.UnregisterCallback<PointerDownEvent>(OnLeftWinkButtonDown);
            leftWinkButton.UnregisterCallback<PointerUpEvent>(OnLeftWinkButtonUp);
            rightWinkButton.UnregisterCallback<PointerDownEvent>(OnRightWinkButtonDown);
            rightWinkButton.UnregisterCallback<PointerUpEvent>(OnRightWinkButtonUp);
            leftWinkButton.UnregisterCallback<ClickEvent>(OnClickedLeftWinkButton);
            rightWinkButton.UnregisterCallback<ClickEvent>(OnClickedRightWinkButton);

            fixationField.UnregisterValueChangedCallback(FixationPositionChangedCallback);
            leftEyeField.UnregisterValueChangedCallback(LeftEyePositionChangedCallback);
            rightEyeField.UnregisterValueChangedCallback(RightEyePositionChangedCallback);

            pupilDistanceField.UnregisterValueChangedCallback(PupilDistanceChangedCallback);
            leftPupilSizeSlider.UnregisterValueChangedCallback(LeftPupilSizeChangedCallback);
            rightPupilSizeSlider.UnregisterValueChangedCallback(RightPupilSizeChangedCallback);
            fixationSlider.UnregisterValueChangedCallback(FixationConfidenceChangedCallback);
            leftEyeSlider.UnregisterValueChangedCallback(LeftEyeConfidenceChangedCallback);
            rightEyeSlider.UnregisterValueChangedCallback(RightEyeConfidenceChangedCallback);
        }

        public event Action<EyeTrackingCalibrationStatus> CalibrationStatusChanged;

        public event Action<EyeTrackingError> ErrorStateChanged;

        public event Action<float> FixationConfidenceChanged;
        public event Action<Vector3> FixationPositionChanged;

        public event Action<bool> FollowHeadPoseChanged;
        public event Action<bool> LeftEyeBlinkChanged;
        public event Action<float> LeftEyeConfidenceChanged;
        public event Action<Vector3> LeftEyePositionChanged;
        public event Action<float> LeftPupilSizeChanged;
        public event Action<int> PupilDistanceChanged;
        public event Action<bool> RightEyeBlinkChanged;
        public event Action<float> RightEyeConfidenceChanged;
        public event Action<Vector3> RightEyePositionChanged;
        public event Action<float> RightPupilSizeChanged;

        public void SetCalibrationStatus(EyeTrackingCalibrationStatus calibrationStatus)
        {
            calibrationStatusField.SetValueWithoutNotify(calibrationStatus);
        }

        public void SetErrorState(EyeTrackingError errorState)
        {
            errorField.SetValueWithoutNotify(errorState);
        }

        public void SetFixationConfidence(float fixationConfidence)
        {
            fixationSlider.SetValueWithoutNotify(fixationConfidence);
        }

        public void SetFixationPosition(Vector3 fixationPosition)
        {
            fixationField.SetValueWithoutNotify(fixationPosition.RoundToDisplay());
        }

        public void SetFollowHeadPose(bool followHeadPose)
        {
            followHeadPoseToggle.SetValueWithoutNotify(followHeadPose);
        }

        public void SetLeftEyeBlink(bool leftEyeBlinkValue)
        {
            SetEyeBlinkStyle(leftWinkButton, leftEyeBlinkValue);
        }

        public void SetLeftEyeConfidence(float leftEyeConfidence)
        {
            leftEyeSlider.SetValueWithoutNotify(leftEyeConfidence);
        }

        public void SetLeftEyePosition(Vector3 leftEyePosition)
        {
            leftEyeField.SetValueWithoutNotify(leftEyePosition.RoundToDisplay());
        }

        public void SetLeftPupilSize(float pupilSize)
        {
            leftPupilSizeSlider.SetValueWithoutNotify(pupilSize);
        }

        public void DisableHybridFields()
        {
            leftEyeField.SetEnabled(false);
            rightEyeField.SetEnabled(false);
        }

        public void ActiveOnDeviceChanged(bool isActiveOnDevice)
        {
            followHeadPoseToggle.SetEnabled(!isActiveOnDevice);
            pupilDistanceField.SetEnabled(!isActiveOnDevice);
            fixationField.SetEnabled(!isActiveOnDevice);
        }

        public void SetPupilDistance(int pupilDistance)
        {
            pupilDistanceField.SetValueWithoutNotify(pupilDistance);
        }

        public void SetRightEyeBlink(bool rightEyeBlinkValue)
        {
            SetEyeBlinkStyle(rightWinkButton, rightEyeBlinkValue);
        }

        public void SetRightEyeConfidence(float rightEyeConfidence)
        {
            rightEyeSlider.SetValueWithoutNotify(rightEyeConfidence);
        }

        public void SetRightEyePosition(Vector3 rightEyePosition)
        {
            rightEyeField.SetValueWithoutNotify(rightEyePosition.RoundToDisplay());
        }

        public void SetRightPupilSize(float pupilSize)
        {
            rightPupilSizeSlider.SetValueWithoutNotify(pupilSize);
        }

        private void FixationConfidenceChangedCallback(ChangeEvent<float> evt)
        {
            FixationConfidenceChanged?.Invoke(evt.newValue);
        }

        private void FixationPositionChangedCallback(ChangeEvent<Vector3> evt)
        {
            FixationPositionChanged?.Invoke(evt.newValue);
        }

        private void FollowHeadPoseChangedCallback(ChangeEvent<bool> evt)
        {
            FollowHeadPoseChanged?.Invoke(evt.newValue);
        }

        private void LeftEyeConfidenceChangedCallback(ChangeEvent<float> evt)
        {
            LeftEyeConfidenceChanged?.Invoke(evt.newValue);
        }

        private void LeftEyePositionChangedCallback(ChangeEvent<Vector3> evt)
        {
            LeftEyePositionChanged?.Invoke(evt.newValue);
        }

        private void LeftPupilSizeChangedCallback(ChangeEvent<float> evt)
        {
            LeftPupilSizeChanged?.Invoke(evt.newValue);
        }

        private void OnCalibrationStatusChangedCallback(ChangeEvent<Enum> evt)
        {
            CalibrationStatusChanged?.Invoke((EyeTrackingCalibrationStatus)evt.newValue);
        }

        private void OnClickedLeftWinkButton(ClickEvent evt)
        {
            if (!EyeTrackingState.IsHoldEnabled)
            {
                return;
            }

            LeftEyeBlink = !LeftEyeBlink;
            if (linkToggle.value)
            {
                RightEyeBlink = LeftEyeBlink;
            }
        }

        private void OnClickedRightWinkButton(ClickEvent evt)
        {
            if (!EyeTrackingState.IsHoldEnabled)
            {
                return;
            }

            RightEyeBlink = !RightEyeBlink;
            if (linkToggle.value)
            {
                LeftEyeBlink = RightEyeBlink;
            }
        }

        private void OnErrorTypeChanged(ChangeEvent<Enum> evt)
        {
            ErrorStateChanged?.Invoke((EyeTrackingError)evt.newValue);
        }

        private void OnEyeBlinkLinkToggle(ChangeEvent<bool> evt)
        {
            EyeTrackingState.IsEyeBlinkLinked = evt.newValue;
            SetWinkButtonsText();
        }

        private void OnHoldToggle(ChangeEvent<bool> evt)
        {
            EyeTrackingState.IsHoldEnabled = evt.newValue;
            SetWinkButtonsText();
            if (!EyeTrackingState.IsHoldEnabled)
            {
                LeftEyeBlink = false;
                RightEyeBlink = false;
            }
        }

        private void OnLeaveLeftWinkButton(PointerLeaveEvent pointerLeaveEvent)
        {
            if (EyeTrackingState.IsHoldEnabled && pointerLeaveEvent.pressedButtons != 1)
            {
                return;
            }

            LeftEyeBlink = false;
            if (EyeTrackingState.IsEyeBlinkLinked)
            {
                RightEyeBlink = false;
            }
        }

        private void OnLeaveRightWinkButton(PointerLeaveEvent pointerLeaveEvent)
        {
            if (EyeTrackingState.IsHoldEnabled && pointerLeaveEvent.pressedButtons != 1)
            {
                return;
            }

            RightEyeBlink = false;
            if (EyeTrackingState.IsEyeBlinkLinked)
            {
                LeftEyeBlink = false;
            }
        }

        private void OnLeftWinkButtonDown(PointerDownEvent evt)
        {
            if (EyeTrackingState.IsHoldEnabled)
            {
                return;
            }

            LeftEyeBlink = true;
            if (EyeTrackingState.IsEyeBlinkLinked)
            {
                RightEyeBlink = true;
            }
        }

        private void OnLeftWinkButtonUp(PointerUpEvent evt)
        {
            if (EyeTrackingState.IsHoldEnabled)
            {
                return;
            }

            LeftEyeBlink = false;
            if (EyeTrackingState.IsEyeBlinkLinked)
            {
                RightEyeBlink = false;
            }
        }

        private void OnRightWinkButtonDown(PointerDownEvent evt)
        {
            if (EyeTrackingState.IsHoldEnabled)
            {
                return;
            }

            RightEyeBlink = true;
            if (EyeTrackingState.IsEyeBlinkLinked)
            {
                LeftEyeBlink = true;
            }
        }

        private void OnRightWinkButtonUp(PointerUpEvent evt)
        {
            if (EyeTrackingState.IsHoldEnabled)
            {
                return;
            }

            RightEyeBlink = false;
            if (EyeTrackingState.IsEyeBlinkLinked)
            {
                LeftEyeBlink = false;
            }
        }

        private void PupilDistanceChangedCallback(ChangeEvent<int> evt)
        {
            var pupilDistance = Mathf.Clamp(evt.newValue, 50, 80);
            pupilDistanceField.SetValueWithoutNotify(pupilDistance);
            PupilDistanceChanged?.Invoke(pupilDistance);
        }

        private void RightEyeConfidenceChangedCallback(ChangeEvent<float> evt)
        {
            RightEyeConfidenceChanged?.Invoke(evt.newValue);
        }

        private void RightEyePositionChangedCallback(ChangeEvent<Vector3> evt)
        {
            RightEyePositionChanged?.Invoke(evt.newValue);
        }

        private void RightPupilSizeChangedCallback(ChangeEvent<float> evt)
        {
            RightPupilSizeChanged?.Invoke(evt.newValue);
        }

        private void SetEyeBlinkStyle(VisualElement button, bool blink)
        {
            if (blink)
            {
                button.AddToClassList("wink-button-selected");
            }
            else if (button.ClassListContains("wink-button-selected"))
            {
                button.RemoveFromClassList("wink-button-selected");
            }
        }
    }
}
