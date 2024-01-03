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
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class DeviceRenderViewPresenter : RenderViewPresenter<DeviceRenderViewState>
    {
        public static readonly string connectionStatusMessageDeviceMode =
            "Please start an MLSDK application and watch it on the device.";
        public static readonly string connectionStatusMessageNonDeviceMode =
            ZIBridge.IsHybridDisabled() ?
            "Please start a Simulator target to enable this view." :
            "Please start a Simulator or Hybrid target to enable this view.";

        public event Action ResetHeadposeButtonHandler;

        public event Action TwoEyedModeToggleButtonHandler;

        private VisualElement connectionStatusPanel;
        private Button resetHeadposeButton;

        private Button twoEyeModeButton;
        public override PeripheralInputSource InputSource => PeripheralInputSource.DeviceView;

        private new DeviceRenderViewState State
        {
            get => base.State;
            set => base.State = value;
        }

        public override void OnEnable(VisualElement root)
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.magicleap.appsim/ZIFUnity/Panels/RenderWindows/Views/ZIDeviceRenderView.uxml");
            visualTree.CloneTree(root);

            base.OnEnable(root);
            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/RenderWindows/Views/ZIRenderWindowDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/RenderWindows/Views/ZIRenderWindowLightStyle.uss");
        }

        public override void ToggleConnectionStatusPanelDisplay(bool enablePanel)
        {
            connectionStatusPanel.SetDisplay(enablePanel);
        }

        public override void SwitchConnectionStatusMessage(bool isDeviceMode)
        {
            connectionStatusLabel.text = isDeviceMode ? connectionStatusMessageDeviceMode : connectionStatusMessageNonDeviceMode;
        }

        public override void ToggleToolbarButtonsEnabled(bool enableButtons)
        {
            twoEyeModeButton.SetEnabled(enableButtons);
            resetHeadposeButton.SetEnabled(enableButtons);
        }

        protected override void AssertFields()
        {
            base.AssertFields();

            Assert.IsNotNull(twoEyeModeButton, nameof(twoEyeModeButton));
            Assert.IsNotNull(resetHeadposeButton, nameof(resetHeadposeButton));
            Assert.IsNotNull(connectionStatusPanel, nameof(connectionStatusPanel));
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();

            twoEyeModeButton = Root.Q<Button>("TwoEyeModeBtn");
            resetHeadposeButton = Root.Q<Button>("ResetHeadposeBtn");
            connectionStatusPanel = Root.Q<VisualElement>("ConnectionStatusPanel");
        }

        protected override void DeSerialize()
        {
            if (EditorPrefs.HasKey(PlayerPrefsKey))
            {
                string savedJson = EditorPrefs.GetString(PlayerPrefsKey);
                State = JsonUtility.FromJson<DeviceRenderViewState>(savedJson);
            }

            if (State != null)
            {
                return;
            }

            State = new DeviceRenderViewState();
            State.SetDefaultValues();
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();

            twoEyeModeButton.clicked += OnTwoEyeModeButtonClicked;
            resetHeadposeButton.clicked += OnResetHeadposeButtonClicked;
        }

        protected override void Serialize()
        {
            string json = JsonUtility.ToJson(State);
            EditorPrefs.SetString(PlayerPrefsKey, json);
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();

            twoEyeModeButton.clicked -= OnTwoEyeModeButtonClicked;
            resetHeadposeButton.clicked -= OnResetHeadposeButtonClicked;
        }

        private void OnResetHeadposeButtonClicked()
        {
            ResetHeadposeButtonHandler?.Invoke();
            MarkDirtyRepaint();
        }

        private void OnTwoEyeModeButtonClicked()
        {
            TwoEyedModeToggleButtonHandler?.Invoke();
            MarkDirtyRepaint();
        }
    }
}
