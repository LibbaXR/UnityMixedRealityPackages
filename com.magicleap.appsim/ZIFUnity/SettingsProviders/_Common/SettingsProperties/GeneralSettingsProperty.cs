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
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    [Serializable]
    internal class GeneralSettingsProperty
    {
        public string Category;
        public string Description;
        public string Label;
        public string Name;
        public string Type;
        public bool UserVisible;
        [NonSerialized] protected ConfigurationSettings ConfigurationSettings;
        [NonSerialized] protected string DeviceName;
        [NonSerialized] protected string SettingName;

        public static GeneralSettingsProperty CreateSettingsProperty(string deviceName, string settingName, string preDefinedType, ConfigurationSettings configurationSettings)
        {
            string json = configurationSettings.GetPropertiesJson(deviceName, settingName);
            var generalSettings = JsonUtility.FromJson<GeneralSettingsProperty>(json);
            GeneralSettingsProperty result;

            if (preDefinedType == "FloatSpecific")
            {
                result = JsonUtility.FromJson<FloatSpecificSettingsProperty>(json);
            }
            else if (generalSettings.Type == "boolean")
            {
                result = JsonUtility.FromJson<BoolSettingsProperty>(json);
            }
            else if (generalSettings.Type == "Enumeration")
            {
                result = JsonUtility.FromJson<EnumerationSettingsProperty>(json);
            }
            else if (generalSettings.Type == "float")
            {
                result = JsonUtility.FromJson<FloatSettingsProperty>(json);
            }
            else if (generalSettings.Type == "rgb")
            {
                result = JsonUtility.FromJson<RGBSettingsProperty>(json);
            }
            else if (generalSettings.Type is "Position" or "Rotation")
            {
                result = JsonUtility.FromJson<Vector3SettingsProperty>(json);
            }
            else if (generalSettings.Type is "integer" or "Milliseconds")
            {
                result = JsonUtility.FromJson<IntegerSettingsProperty>(json);
            }
            else if (generalSettings.Type == "rgba")
            {
                result = JsonUtility.FromJson<RGBASettingsProperty>(json);
            }
            else
            {
                Debug.LogError("Unsupported settings type: " + generalSettings.Type);
                result = new GeneralSettingsProperty();
            }

            result.ConfigurationSettings = configurationSettings;
            result.SettingName = settingName;
            result.DeviceName = deviceName;

            return result;
        }

        public virtual void RefreshPropertyValue()
        {
            if (!GetValueJson(out string valueJson))
            {
                return;
            }

            SetSettingValue(valueJson);
        }

        public virtual void RenderProperty(VisualElement root)
        {
        }

        protected bool GetValueJson(out string valueJson)
        {
            try
            {
                valueJson = ConfigurationSettings.GetValueJson(DeviceName, SettingName);
            }
            catch (ResultIsErrorException e)
            {
                if (e.Result is Result.DeviceNotFound or Result.DoesNotExist)
                {
                    valueJson = string.Empty;
                    return false;
                }

                Console.WriteLine(e);
                throw;
            }

            return true;
        }

        protected virtual void SetSettingValue(string valueJson)
        {
        }
    }
}
