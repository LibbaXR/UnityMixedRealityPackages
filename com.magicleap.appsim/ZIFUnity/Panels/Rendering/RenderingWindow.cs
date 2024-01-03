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
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class RenderingWindow : PanelWindow
    {
        public static readonly string generalTabName = "General";
        public static readonly string sceneViewTabName = "Scene View";
        public static readonly string deviceViewTabName = "Device View";

        private static readonly string windowName = "App Sim Rendering";

        private readonly SettingsLoader settingsLoader = new( @"Packages\com.magicleap.appsim\ZIFUnity\Panels\Rendering\RenderingSettingsJSON.json");
        private const string pathToDarkUss = @"Packages\com.magicleap.appsim\ZIFUnity\Panels\Rendering\Styles\RenderingViewDarkStyle.uss";
        private const string pathToLightUss = @"Packages\com.magicleap.appsim\ZIFUnity\Panels\Rendering\Styles\RenderingViewLightStyle.uss";

        private List<RenderingWindowTab> categories = new List<RenderingWindowTab>();

        private bool isSessionRunning = false;

        private ZIBridge.ModuleWrapper<ConfigurationSettings, ConfigurationSettingsChanges> ConfigurationSettings => ZIBridge.Instance.ConfigurationSettings;

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Rendering #F10", false, MenuItemPriority_Rendering)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Rendering", isValidateFunction: false, priority: MenuItemPriority_Rendering)]
#endif
        public static void ShowWindow()
        {
            var window = GetWindow<RenderingWindow>(windowName);
            window.minSize = new Vector2(200,250);
            window.maxSize = new Vector2(400,600); 
            window.SetSize(300, 380);
        }

        private void OnEnable()
        {
            ZIBridge.Instance.OnSessionConnectedChanged += OnSessionConnectionStatusChanged;
            ConfigurationSettings.OnHandleConnectionChanged += OnSessionConnectionStatusChanged;
            ConfigurationSettings.OnTakeChanges += ConfigurationSettingsChanged;

            OnSessionConnectionStatusChanged(ZIBridge.IsHandleConnected);
        }
        
        private void OnDisable()
        {
            ZIBridge.Instance.OnSessionConnectedChanged -= OnSessionConnectionStatusChanged;
            ConfigurationSettings.OnHandleConnectionChanged -= OnSessionConnectionStatusChanged;
            ConfigurationSettings.OnTakeChanges -= ConfigurationSettingsChanged;
        }

        private bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && ConfigurationSettings.IsHandleConnected;
        }

        private void OnSessionConnectionStatusChanged(bool status)
        {
            if (AreRequiredModulesConnected() && !isSessionRunning)
            {
                isSessionRunning = true;
            }
            else if (!AreRequiredModulesConnected() && isSessionRunning)
            {
                isSessionRunning = false;
            }

            if (isSessionRunning && categories.Count==0)
            {
                InitializeTabs();
            };

            foreach (RenderingWindowTab renderingViewTab in categories)
            {
                renderingViewTab.ConstructTab(isSessionRunning);
            }
        }

        private void ConfigurationSettingsChanged(ConfigurationSettingsChanges changes)
        {
            if (changes.HasFlag(ConfigurationSettingsChanges.ConfigurationSettingsChanged))
            {
                StringList changesList = StringList.Alloc();
                ConfigurationSettings.Handle.TakeChangedSettings(changesList);
                
                foreach (RenderingWindowTab renderingViewTab in categories)
                {
                    renderingViewTab.UpdateTab(changesList);
                }
            }
        }
        
        private void InitializeTabs()
        {
            string pathToUSS = EditorGUIUtility.isProSkin ? pathToDarkUss : pathToLightUss;
            
            rootVisualElement.styleSheets.Add(AssetDatabase.LoadAssetAtPath<StyleSheet>(pathToUSS));

            ScrollView scrollView = new ScrollView()
            {
                mode = ScrollViewMode.Vertical
            };
            
            rootVisualElement.Add(scrollView);

            categories = new List<RenderingWindowTab>()
            {
                new RenderingWindowTab(scrollView, generalTabName, settingsLoader.GetByCategory(generalTabName)), 
                new RenderingWindowTab(scrollView, sceneViewTabName, settingsLoader.GetByCategory(sceneViewTabName)),
                new RenderingWindowTab(scrollView, deviceViewTabName, settingsLoader.GetByCategory(deviceViewTabName))
            };
            
            foreach (RenderingWindowTab renderingViewTab in categories)
            {
                renderingViewTab.ConstructTab(isSessionRunning);
            }
        }

        private void SetSize(float width, float height)
        {
            Rect windowRect = position;
            windowRect.height = height;
            windowRect.width = width;
            position = windowRect;
        }
    }
}
