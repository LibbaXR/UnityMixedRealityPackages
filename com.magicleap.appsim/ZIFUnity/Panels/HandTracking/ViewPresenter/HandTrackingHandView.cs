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
using ml.zi;
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class HandTrackingHandView
    {
        public event Action<bool> ContentFoldoutChanged;
        public event Action<bool> TransformFoldoutChanged;

        public HandTrackingPoseButton CButton;
        public Slider ConfidenceSlider;

        public Foldout ContentFoldout;

        public Label CurrentPoseLabel;
        public HandTrackingPoseButton FistButton;
        public Toggle FollowHeadPoseToggle;
        public Toggle IsHoldingControlToggle;

        public HandTrackingPoseButton LButton;

        public HandTrackingPoseButton NoPoseButton;
        public HandTrackingPoseButton OkButton;
        public HandTrackingPoseButton OpenButton;
        public Vector3Field OrientationField;
        public HandTrackingPoseButton PinchButton;
        public HandTrackingPoseButton PointButton;
        public Dictionary<string, HandTrackingPoseButton> PoseButtons = new();

        public Toggle PoseHoldToggle;

        public Vector3Field PositionField;
        public HandTrackingPoseButton ThumbButton;
        public Foldout TransformFoldout;

        public Hand Label { get; }
        public bool IsClicked { get; set; }
        public HandTrackingGesture Pose { get; private set; }

        public Button ResetButton;

        public HandTrackingHandView(Hand handLabel)
        {
            Label = handLabel;
        }

        public void AssertFields()
        {
            Assert.IsNotNull(ContentFoldout, nameof(ContentFoldout));
            Assert.IsNotNull(CurrentPoseLabel, nameof(CurrentPoseLabel));
            Assert.IsNotNull(TransformFoldout, nameof(TransformFoldout));
            NoPoseButton.AssertFields();
            PointButton.AssertFields();
            FistButton.AssertFields();
            PinchButton.AssertFields();
            ThumbButton.AssertFields();
            LButton.AssertFields();
            OpenButton.AssertFields();
            OkButton.AssertFields();
            CButton.AssertFields();
            Assert.IsNotNull(PoseHoldToggle, nameof(PoseHoldToggle));
            Assert.IsNotNull(IsHoldingControlToggle, nameof(IsHoldingControlToggle));
            Assert.IsNotNull(FollowHeadPoseToggle, nameof(FollowHeadPoseToggle));
            Assert.IsNotNull(ConfidenceSlider, nameof(ConfidenceSlider));
            Assert.IsNotNull(PositionField, nameof(PositionField));
            Assert.IsNotNull(OrientationField, nameof(OrientationField));
            Assert.IsNotNull(ResetButton,$"{Label}{nameof(ResetButton)}");
        }

        public void BindUIElements(Foldout contentFoldout)
        {
            ContentFoldout = contentFoldout;

            CurrentPoseLabel = ContentFoldout.Q<Label>($"{Label}CurrentPoseValue");
            TransformFoldout = ContentFoldout.Q<Foldout>($"{Label}TransformFoldout");
            PoseHoldToggle = ContentFoldout.Q<Toggle>($"{Label}-Hold-toggle");
            IsHoldingControlToggle = ContentFoldout.Q<Toggle>($"{Label}ControlHoldToggle");
            FollowHeadPoseToggle = ContentFoldout.Q<Toggle>($"{Label}FollowHeadPoseToggle");
            ConfidenceSlider = ContentFoldout.Q<Slider>($"{Label}HandConfidenceSlider");

            BindAndAddHandPoseButton(ref NoPoseButton, $"{Label}NoPoseBtn", HandTrackingGesture.NoGesture);
            BindAndAddHandPoseButton(ref PointButton, $"{Label}-Point-Btn", HandTrackingGesture.Finger);
            BindAndAddHandPoseButton(ref FistButton, $"{Label}-Fist-Btn", HandTrackingGesture.Fist);
            BindAndAddHandPoseButton(ref PinchButton, $"{Label}-Pinch-Btn", HandTrackingGesture.Pinch);
            BindAndAddHandPoseButton(ref ThumbButton, $"{Label}-Thumb-Btn", HandTrackingGesture.Thumb);
            BindAndAddHandPoseButton(ref LButton, $"{Label}-L-Btn", HandTrackingGesture.L);
            BindAndAddHandPoseButton(ref OpenButton, $"{Label}-Open-Btn", HandTrackingGesture.OpenHand);
            BindAndAddHandPoseButton(ref OkButton, $"{Label}-Ok-Btn", HandTrackingGesture.Ok);
            BindAndAddHandPoseButton(ref CButton, $"{Label}-C-Btn", HandTrackingGesture.C);

            PositionField = ContentFoldout.Q<Vector3Field>($"{Label}PositionFields");
            OrientationField = ContentFoldout.Q<Vector3Field>($"{Label}OrientationFields");

            IsHoldingControlToggle.SetEnabled(false);
            TransformFoldout.RegisterValueChangedCallback(OnTransformFoldoutChange);
            ContentFoldout.RegisterValueChangedCallback(OnContentFoldoutChange);

            // hidden for now, may be removed later (SDKUNITY-5318)
            IsHoldingControlToggle.visible = false;
            ResetButton = ContentFoldout.Q<Button>("ResetButton");
        }

        public void OnDisable()
        {
            TransformFoldout.UnregisterValueChangedCallback(OnTransformFoldoutChange);
            ContentFoldout.UnregisterValueChangedCallback(OnContentFoldoutChange);
        }

        public void SelectPoseButton(HandTrackingGesture keyPose)
        {
            Pose = keyPose;

            CurrentPoseLabel.text = keyPose.ToString().Replace("_", " ");
            DeselectAllPoseButtons();
            foreach (KeyValuePair<string, HandTrackingPoseButton> kpb in PoseButtons)
            {
                if (kpb.Value.Pose == keyPose)
                {
                    kpb.Value.VisualElement.AddToClassList("selected");
                    break;
                }
            }
        }

        public void SetConfidence(float value)
        {
            ConfidenceSlider.SetValueWithoutNotify(value);
        }

        public void SetContentFoldout(bool isOpened)
        {
            ContentFoldout.SetValueWithoutNotify(isOpened);
        }

        public void SetFollowHeadPose(bool value)
        {
            FollowHeadPoseToggle.SetValueWithoutNotify(value);
        }

        public void SetHoldingControl(bool value)
        {
            IsHoldingControlToggle.SetValueWithoutNotify(value);
        }

        public void SetOrientation(Vector3 value)
        {
            OrientationField.SetValueWithoutNotify(value.RoundToDisplay());
        }

        public void SetPoseHoldToggle(bool isEnabled)
        {
            PoseHoldToggle.SetValueWithoutNotify(isEnabled);
        }

        public void SetPosition(Vector3 value)
        {
            PositionField.SetValueWithoutNotify(value.RoundToDisplay());
        }
        
        public void SetActiveOnDevice(bool value)
        {
            FollowHeadPoseToggle.SetEnabled(!value);
            PositionField.SetEnabled(!value);
            OrientationField.SetEnabled(!value);
        }

        public void SetInteractable(bool isEnabled)
        {
            ResetButton.SetEnabled(isEnabled);
            CurrentPoseLabel.SetEnabled(isEnabled);
            PoseHoldToggle.SetEnabled(isEnabled);
            ConfidenceSlider.SetEnabled(isEnabled);
            PositionField.SetEnabled(isEnabled);
            OrientationField.SetEnabled(isEnabled);

            NoPoseButton.SetEnabled(isEnabled);
            PointButton.SetEnabled(isEnabled);
            FistButton.SetEnabled(isEnabled);
            PinchButton.SetEnabled(isEnabled);
            ThumbButton.SetEnabled(isEnabled);
            LButton.SetEnabled(isEnabled);
            OpenButton.SetEnabled(isEnabled);
            OkButton.SetEnabled(isEnabled);
            CButton.SetEnabled(isEnabled);
        }

        public void SetTransformFoldout(bool isOpened)
        {
            TransformFoldout.SetValueWithoutNotify(isOpened);
        }

        private void BindAndAddHandPoseButton(ref HandTrackingPoseButton button, string elementName, HandTrackingGesture pose)
        {
            if (PoseButtons.ContainsKey(elementName))
            {
                return;
            }

            button.VisualElement = ContentFoldout.Q<VisualElement>(elementName);
            button.Image = ContentFoldout.Q<VisualElement>(elementName);
            HandTrackingPoseButton poseBtn = new();
            poseBtn.VisualElement = button.VisualElement;
            poseBtn.Image = button.Image;
            poseBtn.Pose = pose;
            PoseButtons.Add(elementName, poseBtn);
        }

        private void DeselectAllPoseButtons()
        {
            foreach (KeyValuePair<string, HandTrackingPoseButton> kvp in PoseButtons)
            {
                VisualElement button = kvp.Value.VisualElement;
                button.RemoveFromClassList("selected");
            }
        }

        private void OnContentFoldoutChange(ChangeEvent<bool> evt)
        {
            ContentFoldoutChanged?.Invoke(ContentFoldout.value);
        }

        private void OnTransformFoldoutChange(ChangeEvent<bool> evt)
        {
            TransformFoldoutChanged?.Invoke(TransformFoldout.value);
        }
    }
}
