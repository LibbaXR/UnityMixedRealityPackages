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
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class PropertiesPresenter : CustomView
    {
        public event Action<string> OnNameFieldChanged = delegate { };
        public event Action<CombinedVector3> OnOrientationFieldChanged = delegate { };
        public event Action<CombinedVector3> OnPositionFieldChanged = delegate { };
        public event Action<CombinedVector3> OnScaleFieldChanged = delegate { };

        private Foldout generalPropertiesFoldout;

        private TextField nameField;
        private CombinedVector3Field orientationField;

        private CombinedVector3Field positionField;
        private CombinedVector3Field scaleField;
        private TextField typeField;

        public LightPropertiesPresenter LightProperties { get; private set; }

        public PropertiesPresenter(VisualElement root)
        {
            Root = root;
            Initialize();
        }

        public void SetPropertiesData(PropertiesViewData viewData)
        {
            typeField.SetPropertyData(viewData.DisplayedType);
            nameField.SetPropertyData(viewData.Name);

            positionField.SetPropertyData(viewData.Position);
            orientationField.SetPropertyData(viewData.Orientation);
            scaleField.SetPropertyData(viewData.Scale);

            LightProperties.SetPropertiesData(viewData.Light);
        }

        public void SetPropertiesFoldout(bool isFoldout)
        {
            generalPropertiesFoldout.SetValueWithoutNotify(isFoldout);
        }

        protected override void AssertFields()
        {
            base.AssertFields();

            Assert.IsNotNull(generalPropertiesFoldout, nameof(generalPropertiesFoldout));

            Assert.IsNotNull(positionField, nameof(positionField));
            Assert.IsNotNull(orientationField, nameof(orientationField));
            Assert.IsNotNull(scaleField, nameof(scaleField));

            Assert.IsNotNull(positionField, nameof(positionField));
            Assert.IsNotNull(orientationField, nameof(orientationField));
            Assert.IsNotNull(scaleField, nameof(scaleField));

            Assert.IsNotNull(nameField, nameof(typeField));
            Assert.IsNotNull(typeField, nameof(typeField));
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();

            LightProperties = new LightPropertiesPresenter(Root.Q("Light-properties"));

            generalPropertiesFoldout = Root.Q<Foldout>("GeneralProperties-foldout");

            positionField = new CombinedVector3Field(Root.Q<IMGUIContainer>("Position-field"), "Position", CombinedVector3.zero);
            orientationField = new CombinedVector3Field(Root.Q<IMGUIContainer>("Orientation-field"), "Orientation", CombinedVector3.zero);
            scaleField = new CombinedVector3Field(Root.Q<IMGUIContainer>("Scale-field"), "Scale", CombinedVector3.zero);

            nameField = Root.Q<TextField>("Name-field");
            typeField = Root.Q<TextField>("Type-field");
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();

            nameField.RegisterValueChangedCallback(e => OnNameFieldChanged?.Invoke(e.newValue));

            positionField.OnValueChanged += value => OnPositionFieldChanged?.Invoke(value);
            orientationField.OnValueChanged += value => OnOrientationFieldChanged?.Invoke(value);
            scaleField.OnValueChanged += value => OnScaleFieldChanged?.Invoke(value);
        }

        public override void ClearFields()
        {
            typeField.SetValueWithoutNotify("");
            nameField.SetValueWithoutNotify("");
            positionField.ResetField();
            orientationField.ResetField();
            scaleField.ResetField();
            LightProperties.ClearFields();
        }
    }
}
