// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using ml.zi;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ProjectSettingsProvider : SettingsProvider
    {
        private const string PathToUss = @"Packages\com.magicleap.appsim\ZIFUnity\SettingsProviders\_Common\ProjectSettingsProviderStyle.uss";
        private static SessionTargetMode currentSessionTarget = SessionTargetMode.Unknown;
        private static bool isSessionRunning;
        private static SettingsLoader settingsesLoader;
        private readonly string category;
        private readonly string errorMessage;

        private readonly Dictionary<string, GeneralSettingsProperty> renderedSettingProperty = new();
        private readonly string settingName;
        private readonly List<SessionTargetMode> supportedTargets;

        private VisualElement root;
        private VisualElement scrollView;

        private ZIBridge.ModuleWrapper<ConfigurationSettings, ConfigurationSettingsChanges> ConfigurationSettings => ZIBridge.Instance.ConfigurationSettings;

        private ZIBridge.DeviceConfigurationModule DeviceConfiguration =
            ZIBridge.instance.DeviceConfiguration;

        public ProjectSettingsProvider(
            string path,
            string category,
            string settingName,
            List<SessionTargetMode> supportedTargets) : base(path,
            SettingsScope.Project)
        {
            if (ZIBridge.IsHybridDisabled())
            {
                supportedTargets.Remove(SessionTargetMode.Hybrid);
            }
            var targets = string.Join(" or ", supportedTargets);
            this.errorMessage = $"Options are visible only when Magic Leap App Simulator is running a {targets} target";

            this.supportedTargets = supportedTargets;
            this.settingName = settingName;
            this.category = category;

            if (settingsesLoader != null)
            {
                return;
            }

            settingsesLoader = new SettingsLoader( @"Packages\com.magicleap.appsim\ZIFUnity\SettingsProviders\_Common\configurationSettingsJSON.json");
        }

        private bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && ConfigurationSettings.IsHandleConnected && DeviceConfiguration.IsHandleConnected;
        }

        public override void OnActivate(string searchContext, VisualElement rootElement)
        {
            base.OnActivate(searchContext, rootElement);

            rootElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(PathToUss));
            root = rootElement;

            InitializeSession();
            RenderView();
        }

        public override void OnDeactivate()
        {
            base.OnDeactivate();

            ZIBridge.Instance.OnSessionConnectedChanged -= OnSessionConnectionStatusChanged;
            ConfigurationSettings.OnTakeChanges -= ConfigurationSettingsChanged;
        }

        private void ConfigurationSettingsChanged(ConfigurationSettingsChanges changes)
        {
            if (!changes.HasFlag(ConfigurationSettingsChanges.ConfigurationSettingsChanged))
            {
                return;
            }

            StringList str = StringList.Alloc();
            ConfigurationSettings.Handle.TakeChangedSettings(str);

            for (uint i = 0; i < str.GetSize(); i++)
            {
                if (renderedSettingProperty.TryGetValue(str.Get(i), out GeneralSettingsProperty property))
                {
                    property.RefreshPropertyValue();
                }
            }
        }

        private void InitializeSession()
        {
            ConfigurationSettings.OnHandleConnectionChanged += OnSessionConnectionStatusChanged;
            DeviceConfiguration.OnHandleConnectionChanged += OnSessionConnectionStatusChanged;
            ZIBridge.Instance.OnSessionConnectedChanged += OnSessionConnectionStatusChanged;
            ConfigurationSettings.OnTakeChanges += ConfigurationSettingsChanged;

            OnSessionConnectionStatusChanged(ZIBridge.IsHandleConnected);
        }

        private bool IsValidTargetMode()
        {
            return supportedTargets.Contains(currentSessionTarget);
        }

        private void OnSessionConnectionStatusChanged(bool sessionConnectionStatus)
        {
            currentSessionTarget = ZIBridge.CurrentTargetMode;

            if (AreRequiredModulesConnected() && !isSessionRunning)
            {
                isSessionRunning = true;
                RenderView();
            }
            else if (!AreRequiredModulesConnected() && isSessionRunning)
            {
                isSessionRunning = false;
                RenderView();
            }
        }

        private void RenderError()
        {
            scrollView.Add(new HelpBox(errorMessage, HelpBoxMessageType.Info));
        }

        private void RenderFields()
        {
            foreach (Setting settingData in settingsesLoader.GetByCategory(category))
            {
                try
                {
                    var generalInfo = GeneralSettingsProperty.CreateSettingsProperty(settingData.DeviceName, settingData.SettingName, settingData.Type, ConfigurationSettings.Handle);
                    generalInfo.RenderProperty(scrollView);
                    renderedSettingProperty.Add(settingData.SettingName, generalInfo);
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

        private void RenderLabel()
        {
            Label label = new(settingName);
            label.AddToClassList("settings-label");

            scrollView.Add(label);
        }

        private void RenderResetButton()
        {
            Button btn = new() {text = "Reset to Defaults"};
            btn.AddToClassList("reset-button");
            btn.clicked += ResetToDefaultValues;

            scrollView.Add(btn);
        }

        private void RenderView()
        {
            renderedSettingProperty.Clear();
            root.Clear();

            scrollView = new ScrollView {mode = ScrollViewMode.VerticalAndHorizontal};
            root.Add(scrollView);

            RenderLabel();

            if (isSessionRunning && IsValidTargetMode())
            {
                try
                {
                    RenderResetButton();
                    RenderFields();
                }
                catch (ResultIsErrorException e)
                {
                    isSessionRunning = false;
                    currentSessionTarget = SessionTargetMode.Unknown;

                    RenderError();

                    if (e.Result != Result.SessionNotConnected)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            else
            {
                RenderError();
            }
        }

        private void ResetToDefaultValues()
        {
            if (!ConfigurationSettings.IsHandleConnected)
                return;

            foreach (Setting settingData in settingsesLoader.GetByCategory(category))
            {
                try
                {
                    ConfigurationSettings.Handle.ResetValue(settingData.DeviceName, settingData.SettingName);
                }
                catch (ResultIsErrorException e)
                {
                    if (e.Result != Result.DeviceNotFound && e.Result != Result.DoesNotExist)
                    {
                        Debug.LogError(e);
                    }
                }
            }
            DeviceConfiguration.Save();

            RenderView();
        }
    }
}
