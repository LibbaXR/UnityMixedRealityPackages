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
using System.Linq;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class EnumerationSettingsProperty : GeneralSettingsProperty
    {
        [Serializable]
        public class EnumerantsData
        {
            public string Label;
            public string Name;
        }

        public string Default;
        public List<EnumerantsData> Enumerants;

        private DropdownField dropDownField;

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJson))
            {
                dropDownField = new DropdownField {label = Label, tooltip = Description};
                dropDownField.RegisterValueChangedCallback(OnValueChange);
                dropDownField.choices = Enumerants.Select(e => e.Name.Replace("\"", "")).ToList();
                dropDownField.AddToClassList("dropdown-text-align");
                dropDownField.AddToClassList("extended-width");

                SetSettingValue(valueJson);

                root.Add(dropDownField);
            }
        }

        protected override void SetSettingValue(string valueJson)
        {
            dropDownField.SetValueWithoutNotify(valueJson.Replace("\"", ""));
        }

        private void OnValueChange(ChangeEvent<string> evt)
        {
            ConfigurationSettings.SetValueStr(DeviceName, SettingName, evt.newValue);
        }
    }
}
