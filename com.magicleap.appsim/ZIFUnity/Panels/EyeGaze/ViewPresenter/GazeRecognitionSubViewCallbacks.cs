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
    internal class GazeBehaviorButton
    {
        public GazeBehaviorButton(Button button, GazeRecognitionBehavior behavior, Func<bool> holdPredicate)
        {
            Button = button;
            Behavior = behavior;
            HoldPredicate = holdPredicate;
        }

        public Button Button { get; }

        public GazeRecognitionBehavior Behavior { get; }

        private Func<bool> HoldPredicate { get; }

        public event Action<GazeRecognitionBehavior> OnBehaviorSelected;
        public event Action<GazeRecognitionBehavior> OnBehaviorCleared;

        public void RegisterEvents()
        {
            Button.RegisterCallback<PointerDownEvent>(OnPointerDown, TrickleDown.TrickleDown);
            Button.RegisterCallback<PointerUpEvent>(OnPointerUp);
            Button.RegisterCallback<ClickEvent>(OnClick);
        }

        private void OnPointerUp(PointerUpEvent evt)
        {
            if (HoldPredicate())
            {
                return;
            }

            OnBehaviorCleared?.Invoke(Behavior);
        }

        private void OnPointerDown(PointerDownEvent evt)
        {
            if (HoldPredicate())
            {
                return;
            }

            OnBehaviorSelected?.Invoke(Behavior);
        }


        private void OnClick(ClickEvent evt)
        {
            if (!HoldPredicate())
            {
                return;
            }

            OnBehaviorSelected?.Invoke(Behavior);
        }

        public void UnRegisterEvents()
        {
            Button.UnregisterCallback<PointerUpEvent>(OnPointerUp);
            Button.UnregisterCallback<ClickEvent>(OnClick);
            Button.UnregisterCallback<PointerDownEvent>(OnPointerDown);
        }

        public void SelectButton()
        {
            Button.AddToClassList("wink-button-selected");
        }

        public void ClearSelection()
        {
            Button.RemoveFromClassList("wink-button-selected");
        }
    }

    internal partial class GazeRecognitionSubView
    {
        private bool IsGazeBehaviorHeld => holdToggle.value;

        public void RegisterUICallbacks()
        {
            foreach (var button in gazeButtons)
            {
                button.RegisterEvents();
            }

            leftPupilField.RegisterValueChangedCallback(LeftPupilValueChangedCallback);
            rightPupilField.RegisterValueChangedCallback(RightPupilValueChangedCallback);
            onsetField.RegisterValueChangedCallback(OnsetValueChangedCallback);
            durationField.RegisterValueChangedCallback(DurationValueChangedCallback);
            velocitySlider.RegisterValueChangedCallback(VelocityValueChangedCallback);
            amplitudeSlider.RegisterValueChangedCallback(AmplitudeValueChangedCallback);
            directionSlider.RegisterValueChangedCallback(DirectionValueChangedCallback);
            grErrorField.RegisterValueChangedCallback(ErrorChangedCallback);
            holdToggle.RegisterValueChangedCallback(HoldValueChangedHandler);
        }

        public void UnRegisterUICallbacks()
        {
            foreach (var button in gazeButtons)
            {
                button.UnRegisterEvents();
            }

            leftPupilField.UnregisterValueChangedCallback(LeftPupilValueChangedCallback);
            rightPupilField.UnregisterValueChangedCallback(RightPupilValueChangedCallback);
            onsetField.UnregisterValueChangedCallback(OnsetValueChangedCallback);
            durationField.UnregisterValueChangedCallback(DurationValueChangedCallback);
            velocitySlider.UnregisterValueChangedCallback(VelocityValueChangedCallback);
            amplitudeSlider.UnregisterValueChangedCallback(AmplitudeValueChangedCallback);
            directionSlider.UnregisterValueChangedCallback(DirectionValueChangedCallback);
            grErrorField.UnregisterValueChangedCallback(ErrorChangedCallback);
        }

        public event Action<float> GazeDurationChanged;
        public event Action<float> GazeVelocityChanged;
        public event Action<float> GazeOnSetChanged;
        public event Action<float> GazeDirectionChanged;
        public event Action<float> GazeAmplitudeChanged;

        public event Action<Vector2> GazeLeftPupilPositionChanged;
        public event Action<Vector2> GazeRightPupilPositionChanged;

        public event Action<GazeRecognitionBehavior> GazeBehaviorChanged;
        public event Action<GazeRecognitionError> GazeErrorChanged;

        private void BindGazeButtons()
        {
            gazeButtons.Clear();
            gazeButtonTable.Clear();
            var gazeStates = (GazeRecognitionBehavior[])Enum.GetValues(typeof(GazeRecognitionBehavior));
            for (var i = 0; i < stateButtons.Length; i++)
            {
                var stateButton = stateButtons[i];
                stateButton.text = GetSanitizedLabel(gazeStates[i].ToString());
                var gazeButton = new GazeBehaviorButton(stateButton, gazeStates[i], () => IsGazeBehaviorHeld);
                gazeButton.OnBehaviorCleared += GazeButtonClearedHandlerWithNotify;
                gazeButton.OnBehaviorSelected += GazeButtonSelectedHandlerWithNotify;
                gazeButtons.Add(gazeButton);
                gazeButtonTable[gazeStates[i]] = gazeButton;
            }
        }

        private void HoldValueChangedHandler(ChangeEvent<bool> evt)
        {
            if (!evt.newValue)
            {
                SetGazeBehavior(GazeRecognitionBehavior.Unknown);
            }
        }

        private void GazeButtonSelectedHandlerWithNotify(GazeRecognitionBehavior gazeRecognitionBehavior)
        {
            GazeButtonSelectedHandler(gazeRecognitionBehavior, true);
        }

        private void GazeButtonClearedHandlerWithNotify(GazeRecognitionBehavior behavior)
        {
            GazeButtonClearedHandler(behavior, true);
        }

        private void GazeButtonSelectedHandler(GazeRecognitionBehavior gazeRecognitionBehavior, bool notify)
        {
            foreach (var button in gazeButtons)
            {
                if (button.Behavior == gazeRecognitionBehavior)
                {
                    button.SelectButton();
                }
                else
                {
                    button.ClearSelection();
                }
            }

            UpdateGazeMovementFields(gazeRecognitionBehavior);
            if (notify)
            {
                GazeBehaviorChanged?.Invoke(gazeRecognitionBehavior);
            }
        }

        private void GazeButtonClearedHandler(GazeRecognitionBehavior gazeRecognitionBehavior, bool notify)
        {
            var button = gazeButtonTable[gazeRecognitionBehavior];
            button.ClearSelection();
            UpdateGazeMovementFields(GazeRecognitionBehavior.Unknown);
            if (!notify)
            {
                return;
            }

            GazeBehaviorChanged?.Invoke(GazeRecognitionBehavior.Unknown);
        }

        private void UpdateGazeMovementFields(GazeRecognitionBehavior behavior)
        {
            gazeMovementRoot.SetEnabled(behavior is GazeRecognitionBehavior.Pursuit or GazeRecognitionBehavior.Saccade);
        }

        private void AmplitudeValueChangedCallback(ChangeEvent<float> evt)
        {
            GazeAmplitudeChanged?.Invoke(evt.newValue);
        }

        private void DurationValueChangedCallback(ChangeEvent<float> evt)
        {
            GazeDurationChanged?.Invoke(evt.newValue);
        }

        private void DirectionValueChangedCallback(ChangeEvent<float> evt)
        {
            GazeDirectionChanged?.Invoke(evt.newValue);
        }

        private void OnsetValueChangedCallback(ChangeEvent<float> evt)
        {
            GazeOnSetChanged?.Invoke(evt.newValue);
        }

        private void VelocityValueChangedCallback(ChangeEvent<float> evt)
        {
            GazeVelocityChanged?.Invoke(evt.newValue);
        }

        private void LeftPupilValueChangedCallback(ChangeEvent<Vector2> evt)
        {
            GazeLeftPupilPositionChanged?.Invoke(evt.newValue);
        }

        private void RightPupilValueChangedCallback(ChangeEvent<Vector2> evt)
        {
            GazeRightPupilPositionChanged?.Invoke(evt.newValue);
        }

        private void ErrorChangedCallback(ChangeEvent<Enum> evt)
        {
            GazeErrorChanged?.Invoke((GazeRecognitionError)evt.newValue);
        }

        public void SetGazeLeftPupilPosition(Vector2 value)
        {
            leftPupilField.SetValueWithoutNotify(value);
        }

        public void SetGazeRightPupilPosition(Vector2 value)
        {
            rightPupilField.SetValueWithoutNotify(value);
        }

        public void SetGazeOnset(float value)
        {
            onsetField.SetValueWithoutNotify(value);
        }

        public void SetGazeDuration(float value)
        {
            durationField.SetValueWithoutNotify(value);
        }

        public void SetGazeVelocity(float value)
        {
            velocitySlider.SetValueWithoutNotify(value);
        }

        public void SetGazeAmplitude(float value)
        {
            amplitudeSlider.SetValueWithoutNotify(value);
        }

        public void SetGazeDirection(float value)
        {
            directionSlider.SetValueWithoutNotify(value);
        }

        public void SetGazeError(GazeRecognitionError error)
        {
            grErrorField.SetValueWithoutNotify(error);
        }

        public void SetGazeBehavior(GazeRecognitionBehavior gazeRecognitionBehavior)
        {
            GazeButtonClearedHandler(gazeRecognitionBehavior, false);
            GazeButtonSelectedHandler(gazeRecognitionBehavior, false);
        }
    }
}
