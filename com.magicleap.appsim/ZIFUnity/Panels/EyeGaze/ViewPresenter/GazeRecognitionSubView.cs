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
using System.Collections.Generic;
using System.Text;
using ml.zi;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal partial class GazeRecognitionSubView : ISubPanelView<EyeGazeViewPresenter, EyeGazeViewState>
    {
        private readonly List<GazeBehaviorButton> gazeButtons = new();
        private readonly Dictionary<GazeRecognitionBehavior, GazeBehaviorButton> gazeButtonTable = new();
        private Slider amplitudeSlider;
        private Foldout behaviorFoldout;
        private Slider directionSlider;
        private FloatField durationField;
        private VisualElement gazeMovementRoot;
        private VisualElement gazePropertiesRoot;
        private Foldout gazeRecognitionFoldout;
        private EnumField grErrorField;
        private Toggle holdToggle;
        private Vector2Field leftPupilField;
        private FloatField onsetField;
        private Vector2Field rightPupilField;
        private Button[] stateButtons;
        private Slider velocitySlider;


        public void BindUIElements()
        {
            gazeRecognitionFoldout = Root.Q<Foldout>("GazeRecognitionFoldout");
            var totalButtonCount = Enum.GetValues(typeof(GazeRecognitionBehavior)).Length;
            stateButtons = new Button[totalButtonCount];
            for (var i = 0; i < totalButtonCount; i++)
            {
                stateButtons[i] = Root.Q<Button>($"GRStateButton{i + 1}");
            }

            gazePropertiesRoot = Root.Q<VisualElement>("GazePropertiesRoot");
            leftPupilField = Root.Q<Vector2Field>("GRLeftPupilPosition");
            rightPupilField = Root.Q<Vector2Field>("GRRightPupilPosition");
            velocitySlider = Root.Q<Slider>("GRVelocitySlider");
            amplitudeSlider = Root.Q<Slider>("GRAmplitudeSlider");
            directionSlider = Root.Q<Slider>("GRDirectionSlider");
            grErrorField = Root.Q<EnumField>("GRErrorEnumField");
            onsetField = Root.Q<FloatField>("GROnsetField");
            durationField = Root.Q<FloatField>("GRDurationField");
            holdToggle = Root.Q<Toggle>("GRHoldToggle");
            gazeMovementRoot = Root.Q<VisualElement>("GazeMovementRoot");
            behaviorFoldout = Root.Q<Foldout>("GRBehavior-foldout");
            BindGazeButtons();
        }

        public void AssertFields()
        {
            for (var i = 0; i < stateButtons.Length; i++)
            {
                Assert.IsNotNull(stateButtons[i], $"{nameof(stateButtons)}{i}");
            }

            Assert.IsNotNull(gazeRecognitionFoldout, nameof(gazeRecognitionFoldout));
            Assert.IsNotNull(behaviorFoldout, nameof(behaviorFoldout));
            Assert.IsNotNull(gazePropertiesRoot, nameof(gazePropertiesRoot));
            Assert.IsNotNull(velocitySlider, nameof(velocitySlider));
            Assert.IsNotNull(amplitudeSlider, nameof(amplitudeSlider));
            Assert.IsNotNull(directionSlider, nameof(directionSlider));
            Assert.IsNotNull(grErrorField, nameof(grErrorField));
            Assert.IsNotNull(onsetField, nameof(onsetField));
            Assert.IsNotNull(durationField, nameof(durationField));
            Assert.IsNotNull(leftPupilField, nameof(leftPupilField));
            Assert.IsNotNull(rightPupilField, nameof(rightPupilField));
            Assert.IsNotNull(holdToggle, nameof(holdToggle));
            Assert.IsNotNull(gazeMovementRoot, nameof(gazeMovementRoot));
        }

        public void ClearFields()
        {
            holdToggle.SetValueWithoutNotify(false);
            velocitySlider.SetValueWithoutNotify(0);
            amplitudeSlider.SetValueWithoutNotify(0);
            directionSlider.SetValueWithoutNotify(0);
            grErrorField.SetValueWithoutNotify(GazeRecognitionError.None);
            foreach (var button in gazeButtons)
            {
                button.ClearSelection();
            }
            leftPupilField.SetValueWithoutNotify(Vector2.zero);
            rightPupilField.SetValueWithoutNotify(Vector2.zero);
            onsetField.SetValueWithoutNotify(0);
            durationField.SetValueWithoutNotify(0);
            
        }

        public void Serialize()
        {
            State.gazeRecognitionState.IsGazeRecognitionFoldoutOpened = gazeRecognitionFoldout.value;
            State.gazeRecognitionState.IsHoldEnabled = holdToggle.value;
            State.gazeRecognitionState.IsBehaviorFoldoutOpened = behaviorFoldout.value;
        }

        public void SynchronizeViewWithState()
        {
            var gazeRecognitionState = State.gazeRecognitionState;
            gazeRecognitionFoldout.value = gazeRecognitionState.IsGazeRecognitionFoldoutOpened;
            behaviorFoldout.value = gazeRecognitionState.IsBehaviorFoldoutOpened;
            holdToggle.value = gazeRecognitionState.IsHoldEnabled;
            grErrorField.Init(GazeRecognitionError.None);
            directionSlider.lowValue = 0;
            directionSlider.highValue = 360;

            velocitySlider.lowValue = 0;
            velocitySlider.highValue = 700;

            amplitudeSlider.lowValue = 0;
            amplitudeSlider.highValue = 35;

            UpdateTooltips();
        }

        public void SetPanelActive(bool isEnabled)
        {
            holdToggle.SetEnabled(isEnabled);
            foreach (var button in stateButtons)
            {
                button.SetEnabled(isEnabled);
            }

            foreach (var child in gazePropertiesRoot.Children())
            {
                child.SetEnabled(isEnabled);
            }
        }


        public EyeGazeViewPresenter Panel { get; set; }
        public EyeGazeViewState State { get; set; }
        public VisualElement Root { get; set; }

        private string GetSanitizedLabel(string name)
        {
            var builder = new StringBuilder();
            foreach (var c in name)
            {
                if (char.IsUpper(c))
                {
                    builder.Append(' ');
                }

                builder.Append(c);
            }

            return builder.ToString().Trim(' ');
        }

        private void UpdateTooltips()
        {
            string GetTooltipForBehavior(GazeRecognitionBehavior behavior)
            {
                switch (behavior)
                {
                    case GazeRecognitionBehavior.Unknown:
                        return "No behavior detected";
                    case GazeRecognitionBehavior.EyesClosed:
                        return "Eyes Closed";
                    case GazeRecognitionBehavior.Blink:
                        return "Both eyes blinking";
                    case GazeRecognitionBehavior.Fixation:
                        return "Eyes focused on one point";
                    case GazeRecognitionBehavior.Pursuit:
                        return "Slow eye movement";
                    case GazeRecognitionBehavior.Saccade:
                        return "Rapid eye movement";
                    case GazeRecognitionBehavior.BlinkLeft:
                        return "Blink left";
                    case GazeRecognitionBehavior.BlinkRight:
                        return "Blink right";
                    default:
                        throw new ArgumentOutOfRangeException(nameof(behavior), behavior, null);
                }
            }

            foreach (var button in gazeButtons)
            {
                button.Button.tooltip = GetTooltipForBehavior(button.Behavior);
            }

            leftPupilField.tooltip = "Left pupil position within eye tracking area (eye camera pixel)";
            rightPupilField.tooltip = "Right pupil position within eye tracking area (eye camera pixel)";
            onsetField.tooltip = "Eye behavior startup from device bootup (seconds)";
            durationField.tooltip = "Eye behavior duration (seconds)";
            velocitySlider.tooltip = "Speed of eye movement (degrees per second)";
            amplitudeSlider.tooltip = "Eye position displacement (degree of visual angle)";
            directionSlider.tooltip = "Direction of eye movement (radial degrees)";
            grErrorField.tooltip = "Error in eye gaze detection";
        }
    }
}
