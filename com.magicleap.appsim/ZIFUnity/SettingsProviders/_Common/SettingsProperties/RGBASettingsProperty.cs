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
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class RGBASettingsProperty : GeneralSettingsProperty
    {
        public float[] Default;
        protected ColorField ColorField;

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJson))
            {
                ColorField = new ColorField {label = Label, tooltip = Description};
                ColorField.RegisterValueChangedCallback(OnValueChange);
                SetSettingValue(valueJson);
                ColorField.AddToClassList("extended-width");

                root.Add(ColorField);
            }
        }

        protected override void SetSettingValue(string valueJson)
        {
            float[] colorValues = new float[4];

            valueJson = valueJson.Remove(0, 1);
            valueJson = valueJson.Remove(valueJson.Length - 2, 1);
            string[] stringValues = valueJson.Split(",");

            for (int i = 0; i < colorValues.Length; i++)
            {
                colorValues[i] = float.Parse(stringValues[i], NumberStyles.Any, CultureInfo.InvariantCulture);
            }

            ColorField.SetValueWithoutNotify(new Color(colorValues[0], colorValues[1], colorValues[2], colorValues[3]));
        }

        private void OnValueChange(ChangeEvent<Color> evt)
        {
            ConfigurationSettings.SetValueVec4(DeviceName, SettingName, evt.newValue.r, evt.newValue.g, evt.newValue.b, evt.newValue.a);
        }
    }
}
