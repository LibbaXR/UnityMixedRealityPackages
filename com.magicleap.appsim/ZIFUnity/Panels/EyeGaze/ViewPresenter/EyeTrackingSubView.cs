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
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable]
    internal partial class EyeTrackingSubView : ISubPanelView<EyeGazeViewPresenter, EyeGazeViewState>
    {
        private EnumField calibrationStatusField;
        private Foldout confidenceFoldout;

        private EnumField errorField;
        private VisualElement eyeBlinkPanel;

        private Vector3Field fixationField;
        private Slider fixationSlider;

        private Toggle followHeadPoseToggle;
        private Toggle holdToggle;
        private Vector3Field leftEyeField;
        private Slider leftEyeSlider;
        private Slider leftPupilSizeSlider;
        private VisualElement leftWinkButton;

        private Label leftWinkLabel;
        private Toggle linkToggle;

        private Foldout positionFoldout;

        private IntegerField pupilDistanceField;
        private Vector3Field rightEyeField;
        private Slider rightEyeSlider;

        private Slider rightPupilSizeSlider;
        private VisualElement rightWinkButton;
        private Label rightWinkLabel;
        private Foldout stateFoldout;

        private EyeGazeViewState.EyeTrackingState EyeTrackingState => State.eyeTrackingState;

        public EyeGazeViewPresenter Panel { get; set; }

        public void AssertFields()
        {
            Assert.IsNotNull(calibrationStatusField, nameof(calibrationStatusField));
            Assert.IsNotNull(errorField, nameof(errorField));

            Assert.IsNotNull(leftWinkButton, nameof(leftWinkButton));
            Assert.IsNotNull(rightWinkButton, nameof(rightWinkButton));

            Assert.IsNotNull(linkToggle, nameof(linkToggle));
            Assert.IsNotNull(holdToggle, nameof(holdToggle));

            Assert.IsNotNull(positionFoldout, nameof(positionFoldout));
            Assert.IsNotNull(confidenceFoldout, nameof(confidenceFoldout));
            Assert.IsNotNull(stateFoldout, nameof(stateFoldout));

            Assert.IsNotNull(fixationField, nameof(fixationField));
            Assert.IsNotNull(leftEyeField, nameof(leftEyeField));
            Assert.IsNotNull(rightEyeField, nameof(rightEyeField));

            Assert.IsNotNull(pupilDistanceField, nameof(pupilDistanceField));
            Assert.IsNotNull(rightPupilSizeSlider, nameof(rightPupilSizeSlider));
            Assert.IsNotNull(leftPupilSizeSlider, nameof(leftPupilSizeSlider));
            Assert.IsNotNull(fixationSlider, nameof(fixationSlider));
            Assert.IsNotNull(leftEyeSlider, nameof(leftEyeSlider));
            Assert.IsNotNull(rightEyeSlider, nameof(rightEyeSlider));
        }

        [field: SerializeField] public EyeGazeViewState State { get; set; }

        public VisualElement Root { get; set; }

        public void BindUIElements()
        {
            eyeBlinkPanel = Root.Q<VisualElement>("EyeBlink-panel");
            errorField = Root.Q<EnumField>("Error-enum");
            calibrationStatusField = Root.Q<EnumField>("Calibration-enum");

            leftWinkButton = Root.Q<VisualElement>("LeftWink-button");
            rightWinkButton = Root.Q<VisualElement>("RightWink-button");

            leftWinkLabel = Root.Q<Label>("LeftWink-label");
            rightWinkLabel = Root.Q<Label>("RightWink-label");

            followHeadPoseToggle = Root.Q<Toggle>("FollowHead-toggle");
            linkToggle = Root.Q<Toggle>("Link-toggle");
            holdToggle = Root.Q<Toggle>("EyeBlink-toggle");

            positionFoldout = Root.Q<Foldout>("Position-foldout");
            confidenceFoldout = Root.Q<Foldout>("Confidence-foldout");
            stateFoldout = Root.Q<Foldout>("State-foldout");

            fixationField = Root.Q<Vector3Field>("Fixation-field");
            leftEyeField = Root.Q<Vector3Field>("LeftEye-field");
            rightEyeField = Root.Q<Vector3Field>("RightEye-field");

            pupilDistanceField = Root.Q<IntegerField>("PupilDistance-field");
            pupilDistanceField.isDelayed = true;

            leftPupilSizeSlider = Root.Q<Slider>("LeftPupilSize-slider");
            rightPupilSizeSlider = Root.Q<Slider>("RightPupilSize-slider");
            fixationSlider = Root.Q<Slider>("Fixation-slider");
            leftEyeSlider = Root.Q<Slider>("LeftEye-slider");
            rightEyeSlider = Root.Q<Slider>("RightEye-slider");
        }

        public void ClearFields()
        {
            fixationField.SetValueWithoutNotify(Vector3.zero);
            leftEyeField.SetValueWithoutNotify(Vector3.zero);
            rightEyeField.SetValueWithoutNotify(Vector3.zero);
            fixationSlider.SetValueWithoutNotify(1);
            leftEyeSlider.SetValueWithoutNotify(1);
            rightEyeSlider.SetValueWithoutNotify(1);
            leftPupilSizeSlider.SetValueWithoutNotify(10);
            rightPupilSizeSlider.SetValueWithoutNotify(10);
            errorField.SetValueWithoutNotify(EyeTrackingError.None);
            calibrationStatusField.SetValueWithoutNotify(EyeTrackingCalibrationStatus.None);
        }

        public void Serialize()
        {
            var eyeTrackingState = State.eyeTrackingState;
            eyeTrackingState.IsPositionTabOpened = positionFoldout.value;
            eyeTrackingState.IsConfidenceTabOpened = confidenceFoldout.value;
            eyeTrackingState.IsStateTabOpened = stateFoldout.value;
        }

        public void SynchronizeViewWithState()
        {
            positionFoldout.value = EyeTrackingState.IsPositionTabOpened;
            confidenceFoldout.value = EyeTrackingState.IsConfidenceTabOpened;
            stateFoldout.value = EyeTrackingState.IsStateTabOpened;

            linkToggle.value = EyeTrackingState.IsEyeBlinkLinked;
            holdToggle.value = EyeTrackingState.IsHoldEnabled;

            errorField.Init(EyeTrackingError.None);
            calibrationStatusField.Init(EyeTrackingCalibrationStatus.None);
        }


        private void SetWinkButtonsText()
        {
            if (EyeTrackingState.IsHoldEnabled && EyeTrackingState.IsEyeBlinkLinked)
            {
                leftWinkLabel.text = "Hold Blink";
                rightWinkLabel.text = "Hold Blink";
            }

            if (EyeTrackingState.IsHoldEnabled && !EyeTrackingState.IsEyeBlinkLinked)
            {
                leftWinkLabel.text = "Hold L Wink";
                rightWinkLabel.text = "Hold R Wink";
            }

            if (!EyeTrackingState.IsHoldEnabled && EyeTrackingState.IsEyeBlinkLinked)
            {
                leftWinkLabel.text = "Blink";
                rightWinkLabel.text = "Blink";
            }

            if (!EyeTrackingState.IsHoldEnabled && !EyeTrackingState.IsEyeBlinkLinked)
            {
                leftWinkLabel.text = "L Wink";
                rightWinkLabel.text = "R Wink";
            }

            leftWinkButton.tooltip = leftWinkLabel.text;
            rightWinkButton.tooltip = rightWinkLabel.text;
        }
    }
}
