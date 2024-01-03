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
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class BoolSettingsProperty : GeneralSettingsProperty
    {
        public bool Default;
        private Toggle toggle;

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJson))
            {
                toggle = new Toggle {label = Label, tooltip = Description};
                SetSettingValue(valueJson);
                toggle.RegisterValueChangedCallback(OnValueChange);
                toggle.AddToClassList("extended-width");
                root.Add(toggle);
            }
        }

        protected override void SetSettingValue(string valueJson)
        {
            bool value = bool.Parse(valueJson);
            toggle.SetValueWithoutNotify(value);
        }

        private void OnValueChange(ChangeEvent<bool> evt)
        {
            ConfigurationSettings.SetValueBool(DeviceName, SettingName, evt.newValue);
        }
    }
}
