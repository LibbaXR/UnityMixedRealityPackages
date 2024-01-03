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
    internal class ImageTrackingBarcodeUPCASubview : ImageTrackingSubView
    {
        private const string SubviewPath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/ImageTracking/Views/ImageTargetBarcodeUPCATemplate.uxml";
        private const int ExpectedLength = 11;

        private TextField text = null;
        private Label textValidationMessage = null;

        public new ImageTargetBuilderWrapper BondedObject => base.BondedObject;

        public ImageTrackingBarcodeUPCASubview(ImageTargetBuilderWrapper bondedObject) : base(bondedObject)
        {
        }

        public override void SetEnabled(bool status)
        {
            text.SetEnabled(status);
        }


        protected override void AssertFields()
        {
            Assert.IsNotNull(text, nameof(text));
            Assert.IsNotNull(textValidationMessage, nameof(textValidationMessage));
        }

        protected override void BindUIElements()
        {
            text = Root.Q<TextField>("TextField");
            textValidationMessage = Root.Q<Label>("TextValidationMessage");
        }

        protected override void Initialize()
        {
            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(SubviewPath);
            Root = template.Instantiate();

            base.Initialize();
        }

        protected override void RegisterUICallbacks()
        {
            text.RegisterValueChangedCallback(OnTextFieldChangedCallback);
        }

        protected override void SynchronizeViewWithState()
        {
            text.SetValueWithoutNotify(BondedObject.Text);
            ValidateText(BondedObject.Text);
        }

        protected override void UnregisterUICallbacks()
        {
            text.UnregisterValueChangedCallback(OnTextFieldChangedCallback);
        }

        private void OnTextFieldChangedCallback(ChangeEvent<string> evt)
        {
            if (ValidateText(evt.newValue))
            {
                BondedObject.Text = evt.newValue;
            }
        }

        private bool ValidateText(string value)
        {
            VisualElement textElement = text.Children().First(x => x.GetClasses().Any(y => y == "unity-base-field__input"));
            if (value.Length < ExpectedLength)
            {
                textElement.AddToClassList("invalid-text-field");
                textValidationMessage.SetDisplay(true);
                textValidationMessage.text = $"Barcode should have {ExpectedLength} digits!";
                return false;
            }
            textElement.RemoveFromClassList("invalid-text-field");
            textValidationMessage.SetDisplay(false);
            return true;
        }
    }
}
