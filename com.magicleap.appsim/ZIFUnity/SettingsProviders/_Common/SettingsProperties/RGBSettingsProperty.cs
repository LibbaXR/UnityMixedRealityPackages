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
    internal class RGBSettingsProperty : GeneralSettingsProperty
    {
        public float[] Default;
        protected ColorField ColorField;

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJson))
            {
                ColorField = new ColorField {label = Label, tooltip = Description};
                ColorField.RegisterValueChangedCallback(OnValueChange);
                ColorField.showAlpha = false;
                ColorField.AddToClassList("extended-width");

                SetSettingValue(valueJson);

                root.Add(ColorField);
            }
        }

        protected override void SetSettingValue(string valueJson)
        {
            float[] colorValues = new float[3];

            valueJson = valueJson.Remove(0, 1);
            valueJson = valueJson.Remove(valueJson.Length - 2, 1);
            string[] stringValues = valueJson.Split(",");

            for (int i = 0; i < 3; i++)
            {
                colorValues[i] = float.Parse(stringValues[i], NumberStyles.Any, CultureInfo.InvariantCulture);
            }

            ColorField.SetValueWithoutNotify(new Color(colorValues[0], colorValues[1], colorValues[2]));
        }

        private void OnValueChange(ChangeEvent<Color> evt)
        {
            ConfigurationSettings.SetValueVec3(DeviceName, SettingName, evt.newValue.r, evt.newValue.g, evt.newValue.b);
        }
    }
}
