// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using NUnit.Framework;
using System.Linq;
using UnityEditor;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ImageTrackingQRSubview : ImageTrackingSubView
    {
        private const string SubviewPath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/ImageTracking/Views/ImageTargetQRTemplate.uxml";

        private TextField textField = null;
        private Label textValidationMessage = null;
        private SliderInt markerLengthSlider = null;

        public new ImageTargetBuilderWrapper BondedObject => base.BondedObject;

        public ImageTrackingQRSubview(ImageTargetBuilderWrapper bondedObject) : base(bondedObject)
        {
        }

        public override void SetEnabled(bool status)
        {
            textField.SetEnabled(status);
        }


        protected override void AssertFields()
        {
            Assert.IsNotNull(textField, nameof(textField));
            Assert.IsNotNull(textValidationMessage, nameof(textValidationMessage));
            Assert.IsNotNull(markerLengthSlider, nameof(markerLengthSlider));
        }

        protected override void BindUIElements()
        {
            textField = Root.Q<TextField>("Text-field");
            textValidationMessage = Root.Q<Label>("TextValidationMessage");
            markerLengthSlider = Root.Q<SliderInt>("MarkerLength-slider");
        }

        protected override void Initialize()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SubviewPath);
            Root = template.Instantiate();
            base.Initialize();
        }

        protected override void RegisterUICallbacks()
        {
            textField.RegisterValueChangedCallback(OnTextFieldChangedCallback);
            markerLengthSlider.RegisterValueChangedCallback(OnMarkerLengthChangedCallback);
        }

        protected override void SynchronizeViewWithState()
        {
            textField.SetValueWithoutNotify(BondedObject.Text);
            ValidateText(BondedObject.Text);
            markerLengthSlider.SetValueWithoutNotify(BondedObject.MarkerLength);
        }

        protected override void UnregisterUICallbacks()
        {
            textField.UnregisterValueChangedCallback(OnTextFieldChangedCallback);
        }

        private void OnTextFieldChangedCallback(ChangeEvent<string> evt)
        {
            if (ValidateText(evt.newValue))
            {
                BondedObject.Text = evt.newValue;
            }
        }

        private void OnMarkerLengthChangedCallback(ChangeEvent<int> evt)
        {
            BondedObject.MarkerLength = evt.newValue;
        }

        private bool ValidateText(string value)
        {
            VisualElement textElement = textField.Children().First(x => x.GetClasses().Any(y => y == "unity-base-field__input"));
            if (string.IsNullOrEmpty(value))
            {
                textElement.AddToClassList("invalid-text-field");
                textValidationMessage.SetDisplay(true);
                textValidationMessage.text = $"Text should not be empty!";
                return false;
            }
            textElement.RemoveFromClassList("invalid-text-field");
            textValidationMessage.SetDisplay(false);
            return true;
        }
    }
}
