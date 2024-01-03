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
    internal class Vector3SettingsProperty : GeneralSettingsProperty
    {
        [Serializable]
        private class Test
        {
            public string[] Array;
        }

        public float[] Default;

        private Vector3Field vector3Field;

        public override void RenderProperty(VisualElement root)
        {
            if (GetValueJson(out string valueJSON))
            {
                vector3Field = new Vector3Field {label = Label, tooltip = Description};
                vector3Field.RegisterValueChangedCallback(OnValueChange);
                vector3Field.AddToClassList("extended-width");
                SetSettingValue(valueJSON);

                root.Add(vector3Field);
            }
        }

        protected override void SetSettingValue(string valueJSON)
        {
            float[] vectorValues = new float[3];

            valueJSON = valueJSON.Remove(0, 1);
            valueJSON = valueJSON.Remove(valueJSON.Length - 2, 1);
            string[] stringValues = valueJSON.Split(",");

            for (int i = 0; i < 3; i++)
            {
                vectorValues[i] = float.Parse(stringValues[i], NumberStyles.Any, CultureInfo.InvariantCulture);
            }

            vector3Field.SetValueWithoutNotify(new Vector3(vectorValues[0], vectorValues[1], vectorValues[2]));
        }

        private void OnValueChange(ChangeEvent<Vector3> evt)
        {
            ConfigurationSettings.SetValueVec3(DeviceName, SettingName, evt.newValue.x, evt.newValue.y, evt.newValue.z);
        }
    }
}
