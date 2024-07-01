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
    internal partial class ControllerViewPresenter
    {
        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();
            touchPad.TouchpadStateChanged += OnTouchpadStateChanged;
            touchPad.TouchPositionChanged += OnTouchpadPositionChanged;

            // toggles
            followHeadPoseToggle.RegisterValueChangedCallback(e => ControllerFollowHeadPoseChanged?.Invoke(e.newValue));
            touchpadHoldToggle.RegisterValueChangedCallback(OnTouchpadHoldToggled);
            actionHoldToggle.RegisterValueChangedCallback(OnActionHoldToggled);
            connectedToggle.RegisterValueChangedCallback(e => InputConnectionChanged?.Invoke(e.newValue));

            // sliders
            appliedForceSlider.RegisterValueChangedCallback(OnAppliedForceChanged);
            triggerSlider.RegisterValueChangedCallback(e => TriggerValueChanged?.Invoke(e.newValue));
            speedField.RegisterValueChangedCallback(e => GestureSpeedChanged?.Invoke(e.newValue));
            distanceField.RegisterValueChangedCallback(e => GestureDistanceChanged?.Invoke(e.newValue));
            radiusSlider.RegisterValueChangedCallback(e => GestureRadiusChanged?.Invoke(e.newValue));

            // buttons
            triggerButton.RegisterCallback<PointerDownEvent>(OnTriggerButtonDown);
            triggerButton.RegisterCallback<PointerUpEvent>(OnTriggerButtonUp);
            triggerButton.RegisterCallback<PointerLeaveEvent>(OnTriggerButtonLeave);

            bumperButton.RegisterCallback<PointerDownEvent>(OnBumperButtonDown);
            bumperButton.RegisterCallback<PointerUpEvent>(OnBumperButtonUp);
            bumperButton.RegisterCallback<PointerLeaveEvent>(OnBumperButtonLeave);

            menuButton.RegisterCallback<PointerDownEvent>(OnMenuButtonDown);
            menuButton.RegisterCallback<PointerUpEvent>(OnMenuButtonUp);
            menuButton.RegisterCallback<PointerLeaveEvent>(OnMenuButtonLeave);

            backButton.RegisterCallback<PointerDownEvent>(OnBackButtonDown);
            backButton.RegisterCallback<PointerUpEvent>(OnBackButtonUp);
            backButton.RegisterCallback<PointerLeaveEvent>(OnBackButtonLeave);

            // vector3s
            controllerPositionField.RegisterValueChangedCallback(e => ControllerPositionChanged?.Invoke(e.newValue));
            controllerOrientationField.RegisterValueChangedCallback(e => ControllerOrientationChanged?.Invoke(e.newValue));
            touchpadPositionField.RegisterValueChangedCallback(OnTouchPadPositionChanged);
            gesturePositionField.RegisterValueChangedCallback(OnGesturePositionChanged);

            snapToHandPoseSelector.RegisterValueChangedCallback(OnSnapToHandPoseModeChanged);
            gestureTypeSelector.RegisterValueChangedCallback(e =>
                GestureTypeChanged?.Invoke((InputControllerTouchpadGestureType) e.newValue));
            gestureStateSelector.RegisterValueChangedCallback(e =>
                GestureStateChanged((InputControllerTouchpadGestureState) e.newValue));
            gestureDirectionSelector.RegisterValueChangedCallback(e =>
                GestureDirectionChanged((InputControllerTouchpadGestureDirection) e.newValue));

            controlTypeSelector.RegisterValueChangedCallback(v =>
                ControllerTypeChanged?.Invoke((InputControllerType) v.newValue));

            angleField.RegisterValueChangedCallback(e => GestureAngleChanged?.Invoke(e.newValue));
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();
            touchPad.TouchpadStateChanged -= OnTouchpadStateChanged;
            touchPad.TouchPositionChanged -= OnTouchpadPositionChanged;

            actionHoldToggle.UnregisterValueChangedCallback(OnActionHoldToggled);

            triggerButton.UnregisterCallback<PointerDownEvent>(OnTriggerButtonDown);
            triggerButton.UnregisterCallback<PointerUpEvent>(OnTriggerButtonUp);
            triggerButton.UnregisterCallback<PointerLeaveEvent>(OnTriggerButtonLeave);

            bumperButton.UnregisterCallback<PointerUpEvent>(OnBumperButtonUp);
            bumperButton.UnregisterCallback<PointerLeaveEvent>(OnBumperButtonLeave);

            menuButton.UnregisterCallback<PointerUpEvent>(OnMenuButtonUp);
            menuButton.UnregisterCallback<PointerLeaveEvent>(OnMenuButtonLeave);

            backButton.UnregisterCallback<PointerUpEvent>(OnBackButtonUp);
            backButton.UnregisterCallback<PointerLeaveEvent>(OnBackButtonLeave);

            snapToHandPoseSelector.UnregisterValueChangedCallback(OnSnapToHandPoseModeChanged);
        }

        private void OnActionHoldToggled(ChangeEvent<bool> evt)
        {
            State.IsActionHoldToggled = evt.newValue;

            triggerText.text = State.IsActionHoldToggled ? "Hold Trigger" : "Trigger";
            bumperText.text = State.IsActionHoldToggled ? "Hold  Bumper" : "Bumper";
            menuText.text = State.IsActionHoldToggled ? "Hold Menu" : "Menu";
            backText.text = State.IsActionHoldToggled ? "Hold Back" : "Back";

            if (!State.IsActionHoldToggled)
            {
                OnTriggerButtonUp(PointerUpEvent.GetPooled());
                OnBumperButtonUp(PointerUpEvent.GetPooled());
                OnMenuButtonUp(PointerUpEvent.GetPooled());
                OnBackButtonUp(PointerUpEvent.GetPooled());
            }
        }

        private void OnAppliedForceChanged(ChangeEvent<float> evt)
        {
            if (IsTouchPadHeld)
            {
                touchpadPositionField.value =
                    new Vector3(touchpadPositionField.value.x, touchpadPositionField.value.y, evt.newValue);
            }
        }

        private void OnBackButtonDown(PointerDownEvent evt)
        {
            if (State.IsActionHoldToggled && State.IsBackButtonHeld)
            {
                ActionButtonStateChanged?.Invoke(InputControllerButton.None, false);
            }
            else
            {
                ActionButtonStateChanged?.Invoke(InputControllerButton.None, true);
            }
        }

        private void OnBackButtonLeave(PointerLeaveEvent evt)
        {
            if (State.IsBackButtonHeld)
            {
                OnBackButtonUp(PointerUpEvent.GetPooled());
            }
        }

        private void OnBackButtonUp(PointerUpEvent evt)
        {
            if (State.IsActionHoldToggled)
            {
                return;
            }

            ActionButtonStateChanged?.Invoke(InputControllerButton.None, false);
        }

        private void OnBumperButtonDown(PointerDownEvent evt)
        {
            if (State.IsActionHoldToggled && State.IsBumperButtonHeld)
            {
                ActionButtonStateChanged?.Invoke(InputControllerButton.Bumper, false);
            }
            else
            {
                ActionButtonStateChanged?.Invoke(InputControllerButton.Bumper, true);
            }
        }

        private void OnBumperButtonLeave(PointerLeaveEvent evt)
        {
            if (State.IsBumperButtonHeld)
            {
                OnBumperButtonUp(PointerUpEvent.GetPooled());
            }
        }

        private void OnBumperButtonUp(PointerUpEvent evt)
        {
            if (State.IsActionHoldToggled)
            {
                return;
            }

            ActionButtonStateChanged?.Invoke(InputControllerButton.Bumper, false);
        }

        private void OnGesturePositionChanged(ChangeEvent<Vector3> evt)
        {
            gesturePositionField.value = new Vector3(
                Mathf.Clamp(gesturePositionField.value.x, -1, 1),
                Mathf.Clamp(gesturePositionField.value.y, -1, 1),
                Mathf.Clamp(gesturePositionField.value.z, 0, 1));
            GesturePositionChanged?.Invoke(gesturePositionField.value);
        }

        private void OnMenuButtonDown(PointerDownEvent evt)
        {
            ActionButtonStateChanged?.Invoke(InputControllerButton.Menu, !(State.IsActionHoldToggled && State.IsMenuButtonHeld));
        }

        private void OnMenuButtonLeave(PointerLeaveEvent evt)
        {
            if (State.IsMenuButtonHeld)
            {
                OnMenuButtonUp(PointerUpEvent.GetPooled());
            }
        }

        private void OnMenuButtonUp(PointerUpEvent evt)
        {
            if (State.IsActionHoldToggled)
            {
                return;
            }

            ActionButtonStateChanged?.Invoke(InputControllerButton.Menu, false);
        }

        private void OnSnapToHandPoseModeChanged(ChangeEvent<Enum> evt)
        {
            SnapToHandPoseChanged?.Invoke((InputControllerSnapToHandposeMode) evt.newValue);
        }

        private void OnTouchpadHoldToggled(ChangeEvent<bool> evt)
        {
            TouchpadStateChanged?.Invoke(IsTouchPadHeld);
            touchPad.IsTouchpadHeldLock = evt.newValue;
            if (!evt.newValue)
            {
                touchPad.ReleaseTouchPad();
            }
        }

        private void OnTouchpadPositionChanged(Vector2 position)
        {
            touchpadPositionField.value = new Vector3(position.x, position.y, touchpadPositionField.value.z);
        }

        private void OnTouchPadPositionChanged(ChangeEvent<Vector3> evt)
        {
            touchpadPositionField.value = new Vector3(
                Mathf.Clamp(touchpadPositionField.value.x, -1, 1),
                Mathf.Clamp(touchpadPositionField.value.y, -1, 1),
                Mathf.Clamp(touchpadPositionField.value.z, 0, 1));
            TouchpadPositionAndForceChanged?.Invoke(TouchpadPosAndForce);
            TouchpadStateChanged?.Invoke(true);
        }

        private void OnTouchpadStateChanged()
        {
            TouchpadStateChanged?.Invoke(IsTouchPadHeld);
        }

        private void OnTriggerButtonDown(PointerDownEvent evt)
        {
            if (State.IsActionHoldToggled && State.IsTriggerButtonHeld)
            {
                TriggerValueChanged?.Invoke(0);
                State.IsTriggerButtonHeld = false;

                triggerButton.RemoveFromClassList("gradientBackground");
                return;
            }

            TriggerValueChanged?.Invoke(1);

            SetTriggerButtonState(true);
        }

        private void OnTriggerButtonLeave(PointerLeaveEvent evt)
        {
            if (State.IsTriggerButtonHeld)
            {
                OnTriggerButtonUp(PointerUpEvent.GetPooled());
            }
        }

        private void OnTriggerButtonUp(PointerUpEvent evt)
        {
            if (State.IsActionHoldToggled)
            {
                return;
            }

            TriggerValueChanged?.Invoke(0);
            State.IsTriggerButtonHeld = false;

            triggerButton.RemoveFromClassList("gradientBackground");
        }
    }
}
