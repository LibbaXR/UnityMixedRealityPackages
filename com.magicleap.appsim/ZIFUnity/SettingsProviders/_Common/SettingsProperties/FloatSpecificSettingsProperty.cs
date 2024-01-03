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
using System.Globalization;
using System.Linq;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class FloatSpecificSettingsProperty : GeneralSettingsProperty
    {
        public float Default;
        public float UpperLimit = float.MaxValue;
        public float LowerLimit = float.MinValue;

        private DropdownField dropDownField;

        private Dictionary<string, float> possibleValues = new()
        {
            {"1", 1},
            {"3 ∕ 4", 0.75f},
            {"2 ∕ 3", 0.666666666666666666666666f},
            {"1 ∕ 2", 0.5f},
            {"1 ∕ 3", 0.333333333333333333333333f},
            {"1 ∕ 4", 0.25f},
            {"1 ∕ 8", 0.125f},
            {"1 ∕ 16", 0.0625f}
        };

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJson))
            {
                dropDownField = new DropdownField {label = Label, tooltip = Description};
                dropDownField.RegisterValueChangedCallback(OnValueChange);
                dropDownField.choices = possibleValues.Keys.ToList();
                dropDownField.AddToClassList("extended-width");

                SetSettingValue(valueJson);

                root.Add(dropDownField);
            }
        }

        protected override void SetSettingValue(string valueJson)
        {
            float value = float.Parse(valueJson, NumberStyles.Any, CultureInfo.InvariantCulture);

            dropDownField.SetValueWithoutNotify(GetString(value));
        }

        private string GetString(float value)
        {
            foreach (KeyValuePair<string, float> pair in possibleValues.Where(pair => Mathf.Approximately(pair.Value, value)))
            {
                return pair.Key;
            }

            return "1";
        }

        private float GetValue(string text)
        {
            return possibleValues[text];
        }

        private void OnValueChange(ChangeEvent<string> evt)
        {
            ConfigurationSettings.SetValueFloat(DeviceName, SettingName, GetValue(evt.newValue));
        }
    }
}
