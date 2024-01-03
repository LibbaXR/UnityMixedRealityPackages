// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using ml.zi;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class RenderingWindowTab
    {
        private readonly Foldout foldout = null;
        private readonly VisualElement contentContainer = null;
        
        private readonly IEnumerable<Setting> settings;
        private readonly Dictionary<string, GeneralSettingsProperty> settingNameToProperties = new();
        
        private ConfigurationSettings ConfigurationSettings => ZIBridge.Instance.ConfigurationSettings.Handle;
        
        public RenderingWindowTab(VisualElement root, string tabName, IEnumerable<Setting> settings)
        {
            this.foldout = new Foldout()
            {
                text = tabName
            };
            root.Add(foldout);

            contentContainer = foldout.Q<VisualElement>("unity-content");
            
            this.settings = settings;

            ConstructTab(false);
        }
        
        public void ConstructTab(bool readOnly)
        {
            // Only deviceView tab should be active in device mode.
            if (ZIBridge.IsDeviceMode && !foldout.text.Equals(RenderingWindow.deviceViewTabName))
                return;

            settingNameToProperties.Clear();
            contentContainer.Clear();
            
            foldout.SetEnabled(readOnly);
            foldout.value = readOnly;

            if (!readOnly)
            {
                return;
            }

            foreach (Setting settingData in settings)
            {
                try
                {
                    GeneralSettingsProperty generalInfo = GeneralSettingsProperty.CreateSettingsProperty(settingData.DeviceName, settingData.SettingName, settingData.Type, ConfigurationSettings);
                    generalInfo.RenderProperty(contentContainer);
                    settingNameToProperties.Add(settingData.SettingName, generalInfo);
                }
                catch (ResultIsErrorException e)
                {
                    if (e.Result != Result.DeviceNotFound && e.Result != Result.DoesNotExist)
                    {
                        Debug.LogError(e);
                    }
                }
            }
        }

        public void UpdateTab(StringList changesList)
        {
            for (uint i = 0; i < changesList.GetSize(); i++)
            {
                if (settingNameToProperties.TryGetValue(changesList.Get(i), out GeneralSettingsProperty property))
                {
                    property.RefreshPropertyValue();
                }
            }
        }
    }
}
