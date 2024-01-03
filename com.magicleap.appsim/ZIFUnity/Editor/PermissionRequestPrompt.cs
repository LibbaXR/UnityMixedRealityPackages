// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class PermissionRequestPrompt
    {
        private EditorWindow window;

        private string key = default;

        private const int x1dpi = 96;

        private string viewFilePath = "Packages/com.magicleap.appsim/ZIFUnity/Editor/PermissionRequestPrompt.uxml";

        private Label label;
        private Button yesButton;
        private Button cancelButton;

        private Action<bool, string> onGrant;
        private Action<string> onReceivedExternally;

        internal PermissionRequestPrompt(string permissionName,
            ref Action<bool, string> onGranted,
            ref Action<string> onReceivedExternally)
        {
            window = ScriptableObject.CreateInstance<EditorWindow>();

            key = permissionName;
            window.titleContent.text = "Permission Request";
            window.maxSize = new Vector2(300, 75);
            window.minSize = new Vector2(300, 75);
            window.position = new Rect(
                (Screen.currentResolution.width * x1dpi / Screen.dpi - window.minSize.x) / 2,
                (Screen.currentResolution.height * x1dpi / Screen.dpi - window.minSize.y) / 2,
                window.minSize.x,
                window.minSize.y);

            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(viewFilePath);
            visualTree.CloneTree(window.rootVisualElement);
            BindUIElements();
            RegisterUICallbacks();

            label.text = $"Allow {permissionName} permission?";
            this.onGrant = onGranted;
            onReceivedExternally += Grant;

            window.ShowUtility();
            window.Focus();
        }

        private void Grant(string key)
        {
            if (this.key == key)
            {
                onReceivedExternally?.Invoke(key);
                window.Close();
            }
        }

        private void BindUIElements()
        {
            label = window.rootVisualElement.Q<Label>("label");
            yesButton = window.rootVisualElement.Q<Button>("yes-button");
            cancelButton = window.rootVisualElement.Q<Button>("cancel-button");
        }

        private void RegisterUICallbacks()
        {
            yesButton.clicked += OnYesButtonClicked;
            cancelButton.clicked += OnCancelButtonClicked;
        }

        private void OnYesButtonClicked()
        {
            onGrant?.Invoke(true, key);
            window.Close();
        }

        private void OnCancelButtonClicked()
        {
            onGrant?.Invoke(false, key);
            window.Close();
        }
    }
}
