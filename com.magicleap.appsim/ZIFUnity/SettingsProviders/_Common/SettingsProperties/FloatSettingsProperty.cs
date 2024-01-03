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
using System.Globalization;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class FloatSettingsProperty : GeneralSettingsProperty
    {
        public float Default;
        public float UpperLimit = float.MaxValue;
        public float LowerLimit = float.MinValue;

        private FloatField floatField;

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJson))
            {
                floatField = new FloatField {label = Label, tooltip = Description};
                floatField.isDelayed = true;
                SetSettingValue(valueJson);
                floatField.RegisterValueChangedCallback(OnValueChange);
                floatField.AddToClassList("extended-width");
                root.Add(floatField);
            }
        }

        protected override void SetSettingValue(string valueJson)
        {
            float value = float.Parse(valueJson, NumberStyles.Any, CultureInfo.InvariantCulture);

            floatField.SetValueWithoutNotify(value);
        }

        private void OnValueChange(ChangeEvent<float> evt)
        {
            float newValue = Math.Clamp(evt.newValue, LowerLimit, UpperLimit);

            ConfigurationSettings.SetValueFloat(DeviceName, SettingName, newValue);
        }
    }
}
