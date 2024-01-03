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
using UnityEditor.UIElements;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable] 
    internal class IntegerSettingsProperty : GeneralSettingsProperty
    {
        public int Default;
        public int UpperLimit = int.MaxValue;
        public int LowerLimit = int.MinValue;
        public string UnitType;
        public int Priority;

        private IntegerField intField;

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJson))
            {
                intField = new IntegerField {label = Label, tooltip = Description};
                intField.RegisterValueChangedCallback(OnValueChange);
                intField.isDelayed = true;
                intField.AddToClassList("extended-width");
                SetSettingValue(valueJson);

                root.Add(intField);
            }
        }

        protected override void SetSettingValue(string valueJson)
        {
            intField.SetValueWithoutNotify(int.Parse(valueJson));
        }

        private void OnValueChange(ChangeEvent<int> evt)
        {
            int newValue = Math.Clamp(evt.newValue, LowerLimit, UpperLimit);

            ConfigurationSettings.SetValueInt(DeviceName, SettingName, newValue);
        }
    }
}
