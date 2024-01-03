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
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed partial class HandTrackingViewPresenter : ViewPresenter<HandTrackingViewState>
    {
        public HandTrackingHandView LeftHand { get; } = new(Hand.Left);

        public HandTrackingHandView RightHand { get; } = new(Hand.Right);

        public override void OnDisable()
        {
            base.OnDisable();
            LeftHand.OnDisable();
            RightHand.OnDisable();
        }

        public override void OnEnable(VisualElement root)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/HandTracking/Views/ZIHandTrackingView.uxml");
            visualTree.CloneTree(root);

            base.OnEnable(root);
        }

        protected override void AssertFields()
        {
            base.AssertFields();
            LeftHand.AssertFields();
            RightHand.AssertFields();
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();
            LeftHand.BindUIElements(Root.Q<Foldout>("Left-toolbar"));
            RightHand.BindUIElements(Root.Q<Foldout>("Right-toolbar"));
        }

        protected override void SynchronizeViewWithState()
        {
            LeftHand.SetPoseHoldToggle(State.LeftHoldPose);
            LeftHand.SetHoldingControl(false);
            LeftHand.SetContentFoldout(State.LeftHandFoldoutOpened);
            LeftHand.SetTransformFoldout(State.LeftHandTransformFoldoutOpened);

            LeftHand.ContentFoldoutChanged += State.SetLeftHandContentFoldout;
            LeftHand.TransformFoldoutChanged += State.SetLeftHandTransformFoldout;

            RightHand.SetPoseHoldToggle(State.RightHoldPose);
            RightHand.SetHoldingControl(false);
            RightHand.SetContentFoldout(State.RightHandFoldoutOpened);
            RightHand.SetTransformFoldout(State.RightHandTransformFoldoutOpened);

            RightHand.ContentFoldoutChanged += State.SetRightHandContentFoldout;
            RightHand.TransformFoldoutChanged += State.SetRightHandTransformFoldout;
        }

        public override void ClearFields()
        {
            LeftHand.PositionField.SetValueWithoutNotify(Vector3.zero);
            LeftHand.OrientationField.SetValueWithoutNotify(Vector3.zero);
            LeftHand.ConfidenceSlider.SetValueWithoutNotify(0.5f);
            
            RightHand.PositionField.SetValueWithoutNotify(Vector3.zero);
            RightHand.OrientationField.SetValueWithoutNotify(Vector3.zero);
            RightHand.ConfidenceSlider.SetValueWithoutNotify(0.5f);
        }
    }
}
