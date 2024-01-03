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
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal abstract class AlignableTransformViewPresenter<TState> : ResettableTransformViewPresenter<TState>
        where TState : IViewState, new()
    {
        protected Button alignCamera;
        protected Vector3Field orientation;

        protected Vector3Field position;
        public event Action OnAlignDeviceToSceneView;
        public event Action OnAlignSceneToDeviceView;

        public event Action<Quaternion> OnCurrentOrientationChanged;
        public event Action<Vector3> OnCurrentPositionChanged;


        protected override void BindUIElements()
        {
            base.BindUIElements();
            position = Root.Q<Vector3Field>("Position-field");
            orientation = Root.Q<Vector3Field>("Orientation-field");
            alignCamera = Root.Q<Button>("AlignCamera-button");
        }

        protected override void AssertFields()
        {
            base.AssertFields();
            Assert.IsNotNull(alignCamera, nameof(alignCamera));
            Assert.IsNotNull(position, nameof(position));
            Assert.IsNotNull(orientation, nameof(orientation));
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();
            position.RegisterValueChangedCallback(PositionValueChangedCallback);
            orientation.RegisterValueChangedCallback(OrientationValueChangedCallback);
            alignCamera.clicked += AlignCameraButtonOnClicked;
        }

        private void OrientationValueChangedCallback(ChangeEvent<Vector3> evt)
        {
            var quaternion = Quaternion.Euler(evt.newValue);
            OnCurrentOrientationChanged?.Invoke(quaternion);
        }

        private void PositionValueChangedCallback(ChangeEvent<Vector3> evt)
        {
            OnCurrentPositionChanged?.Invoke(evt.newValue);
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();
            position.UnregisterValueChangedCallback(PositionValueChangedCallback);
            orientation.UnregisterValueChangedCallback(OrientationValueChangedCallback);
            alignCamera.clicked -= AlignCameraButtonOnClicked;
        }

        public override void ClearFields()
        {
            base.ClearFields();
            position.SetValueWithoutNotify(Vector3.zero);
            orientation.SetValueWithoutNotify(Vector3.zero);
        }

        private void AlignCameraButtonOnClicked()
        {
            var menu = new GenericMenu();
            menu.AddItem(new GUIContent("Align Scene to Device View"), false, () => OnAlignDeviceToSceneView?.Invoke());
            menu.AddItem(new GUIContent("Align Device to Scene View"), false, () => OnAlignSceneToDeviceView?.Invoke());
            menu.ShowAsContext();
        }

        public void SetPosition(Vector3 newPosition)
        {
            position.SetValueWithoutNotify(newPosition.RoundToDisplay());
        }

        public void SetOrientation(Quaternion newOrientation)
        {
            orientation.SetValueWithoutNotify(newOrientation.eulerAngles.RoundToDisplay());
        }

        public void SetInteractable(bool state)
        {
            Root.SetEnabled(state);
        }
    }
}
