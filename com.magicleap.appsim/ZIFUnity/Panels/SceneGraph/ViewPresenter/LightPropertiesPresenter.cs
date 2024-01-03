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
using NUnit.Framework;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class LightPropertiesPresenter : CustomView
    {
        public event Action<LightPropertiesData> OnLightDataChanged;
        private ColorField lightColorField;
        private Slider lightConeAngleSlider;
        private Slider lightIntensitySlider;
        private FloatField lightRangeField;

        private EnumField lightTypeField;

        public LightPropertiesPresenter(VisualElement root)
        {
            Root = root;
            Initialize();
        }

        public void SetPropertiesData(PropertyData<LightPropertiesData> viewData)
        {
            Root.SetDisplay(viewData.enabled);

            if (viewData.enabled)
            {
                lightColorField.SetValueWithoutNotify(viewData.value.Color);

                lightTypeField.SetValueWithoutNotify(viewData.value.Type);
                lightIntensitySlider.SetValueWithoutNotify(viewData.value.Intensity);

                lightRangeField.SetEnabled(viewData.value.Type != LightType.Directional);
                lightConeAngleSlider.SetEnabled(viewData.value.Type == LightType.Spot);

                switch (viewData.value.Type)
                {
                    case LightType.Point:
                        lightRangeField.SetValueWithoutNotify(viewData.value.Range);
                        break;
                    case LightType.Spot:
                        lightRangeField.SetValueWithoutNotify(viewData.value.Range);
                        lightConeAngleSlider.SetValueWithoutNotify(viewData.value.ConeAngle);
                        break;
                }
            }
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(lightTypeField, nameof(lightTypeField));
            Assert.IsNotNull(lightColorField, nameof(lightColorField));
            Assert.IsNotNull(lightIntensitySlider, nameof(lightIntensitySlider));
            Assert.IsNotNull(lightRangeField, nameof(lightRangeField));
            Assert.IsNotNull(lightConeAngleSlider, nameof(lightConeAngleSlider));
        }

        protected override void BindUIElements()
        {
            lightTypeField = Root.Q<EnumField>("Light-type");
            lightColorField = Root.Q<ColorField>("Light-color");
            lightIntensitySlider = Root.Q<Slider>("Light-intensity");
            lightRangeField = Root.Q<FloatField>("Light-range");
            lightConeAngleSlider = Root.Q<Slider>("Light-angle");

            lightTypeField.Init(UnityEngine.LightType.Directional);
        }

        protected override void RegisterUICallbacks()
        {
            lightTypeField.RegisterValueChangedCallback(e => OnLightDataChanged?.Invoke(GetPropertyViewData()));
            lightColorField.RegisterValueChangedCallback(e => OnLightDataChanged?.Invoke(GetPropertyViewData()));
            lightIntensitySlider.RegisterValueChangedCallback(e => OnLightDataChanged?.Invoke(GetPropertyViewData()));
            lightRangeField.RegisterValueChangedCallback(LightRangeChanged);
            lightConeAngleSlider.RegisterValueChangedCallback(e => OnLightDataChanged?.Invoke(GetPropertyViewData()));
        }

        private LightPropertiesData GetPropertyViewData()
        {
            return new LightPropertiesData
            {
                Type = (LightType) lightTypeField.value,
                Color = lightColorField.value,
                Intensity = lightIntensitySlider.value,
                Range = lightRangeField.value,
                ConeAngle = lightConeAngleSlider.value
            };
        }

        private void LightRangeChanged(ChangeEvent<float> evt)
        {
            if (lightRangeField.value < 0.01f)
            {
                lightRangeField.SetValueWithoutNotify(0.01f);
            }

            OnLightDataChanged?.Invoke(GetPropertyViewData());
        }

        public override void ClearFields()
        {
            Root.SetDisplay(false);
        }
    }
}
