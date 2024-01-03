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
    internal sealed partial class HandTrackingViewPresenter
    {
        public event Action<bool> OnLeftFollowHeadPoseChange;
        public event Action<float> OnLeftHandConfidenceChange;
        public event Action<bool> OnLeftIsHoldingControlChange;
        public event Action<HandTrackingGesture> OnLeftKeyPoseSelected;
        public event Action<Vector3> OnLeftNormHandCenterChange;
        public event Action<Vector3> OnLeftOrientationChange;
        public event Action<Vector3> OnLeftPositionChange;
        public event Action<bool> OnRightFollowHeadPoseChange;
        public event Action<float> OnRightHandConfidenceChange;
        public event Action<bool> OnRightIsHoldingControlChange;

        public event Action<HandTrackingGesture> OnRightKeyPoseSelected;
        public event Action<Vector3> OnRightNormHandCenterChange;
        public event Action<Vector3> OnRightOrientationChange;
        public event Action<Vector3> OnRightPositionChange;

        public event Action OnLeftHandResetCenter;
        public event Action OnRightHandResetCenter;

        public void OnLeftHandConfidenceChanged(float newVal)
        {
            LeftHand.ConfidenceSlider.SetValueWithoutNotify(newVal);
        }

        public void OnLeftHandFollowHeadPoseChanged(bool newVal)
        {
            LeftHand.FollowHeadPoseToggle.SetValueWithoutNotify(newVal);
        }

        public void OnLeftHandIsHoldingControlChanged(bool newVal)
        {
            LeftHand.IsHoldingControlToggle.SetValueWithoutNotify(newVal);
        }

        public void OnLeftHandOrientationChanged(Vector3 newVal)
        {
            LeftHand.SetOrientation(newVal);
        }

        public void OnLeftHandPositionChanged(Vector3 newVal)
        {
            LeftHand.SetPosition(newVal);
        }

        public void OnLeftHandTrackingKeyPoseChanged(HandTrackingGesture newVal)
        {
            LeftHand.SelectPoseButton(newVal);
        }

        public void OnRightHandConfidenceChanged(float newVal)
        {
            RightHand.ConfidenceSlider.SetValueWithoutNotify(newVal);
        }

        public void OnRightHandFollowHeadPoseChanged(bool newVal)
        {
            RightHand.FollowHeadPoseToggle.SetValueWithoutNotify(newVal);
        }

        public void OnRightHandIsHoldingControlChanged(bool newVal)
        {
            RightHand.IsHoldingControlToggle.SetValueWithoutNotify(newVal);
        }

        public void OnRightHandOrientationChanged(Vector3 newVal)
        {
            RightHand.SetOrientation(newVal);
        }

        public void OnRightHandPositionChanged(Vector3 newVal)
        {
            RightHand.SetPosition(newVal);
        }

        public void OnRightHandTrackingKeyPoseChanged(HandTrackingGesture newVal)
        {
            RightHand.SelectPoseButton(newVal);
        }

        public void SetInteractable(bool isEnabled)
        {
            LeftHand.SetInteractable(isEnabled);
            RightHand.SetInteractable(isEnabled);
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();
            LeftHand.ResetButton.clicked += LeftHandResetCenter;
            RightHand.ResetButton.clicked += RightHandResetCenter;
            
            foreach (HandTrackingPoseButton kvp in LeftHand.PoseButtons.Values)
            {
                VisualElement button = kvp.VisualElement;

                button.RegisterCallback<PointerDownEvent, HandTrackingGesture>(OnLeftWinkButtonDown, kvp.Pose);
                button.RegisterCallback<PointerUpEvent, HandTrackingGesture>(OnLeftWinkButtonUp, kvp.Pose);
                button.RegisterCallback<PointerLeaveEvent, HandTrackingGesture>(OnLeftWinkButtonPointerExit, kvp.Pose);
            }

            LeftHand.PoseHoldToggle.RegisterValueChangedCallback(LeftHoldPoseToggle);
            LeftHand.IsHoldingControlToggle.RegisterValueChangedCallback(LeftIsHoldingControlToggle);
            LeftHand.FollowHeadPoseToggle.RegisterValueChangedCallback(LeftHandTransformFollowHeadPoseToggle);

            LeftHand.ConfidenceSlider.RegisterValueChangedCallback(LeftHandConfidenceValueChanged);

            LeftHand.PositionField.RegisterValueChangedCallback(LeftHandPositionValueChanged);
            LeftHand.OrientationField.RegisterValueChangedCallback(LeftHandOrientationValueChanged);

            foreach (HandTrackingPoseButton kvp in RightHand.PoseButtons.Values)
            {
                VisualElement button = kvp.VisualElement;

                button.RegisterCallback<PointerDownEvent, HandTrackingGesture>(OnRightWinkButtonDown, kvp.Pose);
                button.RegisterCallback<PointerUpEvent, HandTrackingGesture>(OnRightWinkButtonUp, kvp.Pose);
                button.RegisterCallback<PointerLeaveEvent, HandTrackingGesture>(OnRightWinkButtonPointerExit, kvp.Pose);
            }

            RightHand.PoseHoldToggle.RegisterValueChangedCallback(RightHoldPoseToggle);
            RightHand.IsHoldingControlToggle.RegisterValueChangedCallback(RightIsHoldingControlToggle);
            RightHand.FollowHeadPoseToggle.RegisterValueChangedCallback(RightHandTransformFollowHeadPoseToggle);

            RightHand.ConfidenceSlider.RegisterValueChangedCallback(RightHandConfidenceValueChanged);

            RightHand.PositionField.RegisterValueChangedCallback(RightHandPositionValueChanged);
            RightHand.OrientationField.RegisterValueChangedCallback(RightHandOrientationValueChanged);
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();
            LeftHand.ResetButton.clicked -= LeftHandResetCenter;
            RightHand.ResetButton.clicked -= RightHandResetCenter;
            foreach (HandTrackingPoseButton kvp in LeftHand.PoseButtons.Values)
            {
                VisualElement button = kvp.VisualElement;

                button.UnregisterCallback<PointerDownEvent, HandTrackingGesture>(OnLeftWinkButtonDown);
                button.UnregisterCallback<PointerUpEvent, HandTrackingGesture>(OnLeftWinkButtonUp);
                button.UnregisterCallback<PointerLeaveEvent, HandTrackingGesture>(OnLeftWinkButtonPointerExit);
            }

            LeftHand.PoseHoldToggle.UnregisterValueChangedCallback(LeftHoldPoseToggle);
            LeftHand.IsHoldingControlToggle.UnregisterValueChangedCallback(LeftIsHoldingControlToggle);
            LeftHand.FollowHeadPoseToggle.UnregisterValueChangedCallback(LeftHandTransformFollowHeadPoseToggle);

            LeftHand.ConfidenceSlider.UnregisterValueChangedCallback(LeftHandConfidenceValueChanged);

            LeftHand.PositionField.UnregisterValueChangedCallback(LeftHandPositionValueChanged);
            LeftHand.OrientationField.UnregisterValueChangedCallback(LeftHandOrientationValueChanged);

            foreach (HandTrackingPoseButton kvp in RightHand.PoseButtons.Values)
            {
                VisualElement button = kvp.VisualElement;

                button.UnregisterCallback<PointerDownEvent, HandTrackingGesture>(OnRightWinkButtonDown);
                button.UnregisterCallback<PointerUpEvent, HandTrackingGesture>(OnRightWinkButtonUp);
                button.UnregisterCallback<PointerLeaveEvent, HandTrackingGesture>(OnRightWinkButtonPointerExit);
            }

            RightHand.PoseHoldToggle.UnregisterValueChangedCallback(RightHoldPoseToggle);
            RightHand.IsHoldingControlToggle.UnregisterValueChangedCallback(RightIsHoldingControlToggle);
            RightHand.FollowHeadPoseToggle.UnregisterValueChangedCallback(RightHandTransformFollowHeadPoseToggle);

            RightHand.ConfidenceSlider.UnregisterValueChangedCallback(RightHandConfidenceValueChanged);

            RightHand.PositionField.UnregisterValueChangedCallback(RightHandPositionValueChanged);
            RightHand.OrientationField.UnregisterValueChangedCallback(RightHandOrientationValueChanged);
        }

        private void LeftHandConfidenceValueChanged(ChangeEvent<float> evt)
        {
            OnLeftHandConfidenceChange?.Invoke(evt.newValue);
        }

        private void LeftHandOrientationValueChanged(ChangeEvent<Vector3> evt)
        {
            OnLeftOrientationChange?.Invoke(evt.newValue);
        }

        private void LeftHandPositionValueChanged(ChangeEvent<Vector3> evt)
        {
            OnLeftPositionChange?.Invoke(evt.newValue);
        }

        private void LeftHandTransformFollowHeadPoseToggle(ChangeEvent<bool> evt)
        {
            OnLeftFollowHeadPoseChange?.Invoke(evt.newValue);
        }

        private void LeftHoldPoseToggle(ChangeEvent<bool> evt)
        {
            State.LeftHoldPose = evt.newValue;
            if (State.LeftHoldPose)
            {
                return;
            }

            LeftHand.SelectPoseButton(HandTrackingGesture.NoGesture);
            OnLeftKeyPoseSelected?.Invoke(LeftHand.Pose);
        }

        private void LeftIsHoldingControlToggle(ChangeEvent<bool> evt)
        {
            OnLeftIsHoldingControlChange?.Invoke(evt.newValue);
        }

        private void LeftNormHandCenterValueChanged(ChangeEvent<Vector3> evt)
        {
            OnLeftNormHandCenterChange?.Invoke(evt.newValue);
        }

        private void OnLeftWinkButtonDown(PointerDownEvent evt, HandTrackingGesture pose)
        {
            LeftHand.IsClicked = true;
            LeftHand.SelectPoseButton(pose);
            OnLeftKeyPoseSelected?.Invoke(LeftHand.Pose);
        }

        private void OnLeftWinkButtonPointerExit(PointerLeaveEvent evt, HandTrackingGesture pose)
        {
            if (!LeftHand.IsClicked)
            {
                return;
            }

            LeftHand.IsClicked = false;
            if (State.LeftHoldPose)
            {
                return;
            }

            LeftHand.SelectPoseButton(HandTrackingGesture.NoGesture);
            OnLeftKeyPoseSelected?.Invoke(LeftHand.Pose);
        }

        private void OnLeftWinkButtonUp(PointerUpEvent evt, HandTrackingGesture pose)
        {
            LeftHand.IsClicked = false;
            if (State.LeftHoldPose)
            {
                return;
            }

            LeftHand.SelectPoseButton(HandTrackingGesture.NoGesture);
            OnLeftKeyPoseSelected?.Invoke(LeftHand.Pose);
        }

        private void OnRightWinkButtonDown(PointerDownEvent evt, HandTrackingGesture pose)
        {
            RightHand.IsClicked = true;
            RightHand.SelectPoseButton(pose);
            OnRightKeyPoseSelected?.Invoke(RightHand.Pose);
        }

        private void OnRightWinkButtonPointerExit(PointerLeaveEvent evt, HandTrackingGesture pose)
        {
            if (!RightHand.IsClicked)
            {
                return;
            }

            RightHand.IsClicked = false;
            if (State.RightHoldPose)
            {
                return;
            }

            RightHand.SelectPoseButton(HandTrackingGesture.NoGesture);
            OnRightKeyPoseSelected?.Invoke(RightHand.Pose);
        }

        private void OnRightWinkButtonUp(PointerUpEvent evt, HandTrackingGesture pose)
        {
            RightHand.IsClicked = false;
            if (State.RightHoldPose)
            {
                return;
            }

            RightHand.SelectPoseButton(HandTrackingGesture.NoGesture);
            OnRightKeyPoseSelected?.Invoke(RightHand.Pose);
        }

        private void RightHandConfidenceValueChanged(ChangeEvent<float> evt)
        {
            OnRightHandConfidenceChange?.Invoke(evt.newValue);
        }

        private void RightHandOrientationValueChanged(ChangeEvent<Vector3> evt)
        {
            OnRightOrientationChange?.Invoke(evt.newValue);
        }

        private void RightHandPositionValueChanged(ChangeEvent<Vector3> evt)
        {
            OnRightPositionChange?.Invoke(evt.newValue);
        }

        private void LeftHandResetCenter()
        {
            OnLeftHandResetCenter?.Invoke();
        }

        private void RightHandResetCenter()
        {
            OnRightHandResetCenter?.Invoke();
        }

        private void RightHandTransformFollowHeadPoseToggle(ChangeEvent<bool> evt)
        {
            OnRightFollowHeadPoseChange?.Invoke(evt.newValue);
        }

        private void RightHoldPoseToggle(ChangeEvent<bool> evt)
        {
            State.RightHoldPose = evt.newValue;
            if (State.RightHoldPose)
            {
                return;
            }

            RightHand.SelectPoseButton(HandTrackingGesture.NoGesture);
            OnRightKeyPoseSelected?.Invoke(RightHand.Pose);
        }

        private void RightIsHoldingControlToggle(ChangeEvent<bool> evt)
        {
            OnRightIsHoldingControlChange?.Invoke(evt.newValue);
        }

        private void RightNormHandCenterValueChanged(ChangeEvent<Vector3> evt)
        {
            OnRightNormHandCenterChange?.Invoke(evt.newValue);
        }
    }
}
