// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using NUnit.Framework;
using UnityEditor;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed partial class TargetViewPresenter : ViewPresenter<TargetViewState>
    {
        private Label errorLabel;

        private VisualElement currentModeIcon;
        private Label currentSessionLabel;

        private TargetViewState.SelectableTargets currentTargetMode = TargetViewState.SelectableTargets.Simulator;
        private Button sessionDropdownButton;

        private VisualElement startStopIcon;
        private Button targetCameraButton;
        private Button targetConnectButton;

        private EnumField targetEnumField;

        public override void OnEnable(VisualElement root)
        {
            string treeAssetPath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/Target/Views/ZITargetView.uxml";
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(treeAssetPath);
            visualTree.CloneTree(root);

            base.OnEnable(root);
            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/Target/Views/ZITargetDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/Target/Views/ZITargetLightStyle.uss");

            targetEnumField.Init(currentTargetMode);
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(targetConnectButton, nameof(targetConnectButton));
            Assert.IsNotNull(sessionDropdownButton, nameof(sessionDropdownButton));
        }

        protected override void BindUIElements()
        {
            targetConnectButton = Root.Q<Button>("TargetConnect-button");
            targetCameraButton = Root.Q<Button>("PrimaryNavTargetSelector");
            sessionDropdownButton = Root.Q<Button>("SessionDropDownButton");
            targetEnumField = Root.Q<EnumField>("TargetDropdown");
            startStopIcon = Root.Q<VisualElement>("StartStopIcon");
            currentModeIcon = Root.Q<VisualElement>("CurrentIcon");
            currentSessionLabel = Root.Q<Label>("CurrentSessionLabel");
            errorLabel = Root.Q<Label>("ErrorLabel");

            InitModeMenu();
        }

        protected override void SynchronizeViewWithState()
        {
            currentTargetMode = State.TargetMode;
        }
    }
}
