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
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class SavingPermissionProfilePrompt : EditorWindow
    {
        private const int x1dpi = 96;
        
        private string viewFilePath = "Packages/com.magicleap.appsim/ZIFUnity/Editor/SavingPermissionProfilePrompt.uxml";
        
        private TextField profileNameInput;
        private Button yesButton;
        private Button cancelButton;

        private string result = null;
        private Action<string> onComplete;

        public static void ShowPrompt(Action<string> onComplete)
        {
            var window = CreateInstance<SavingPermissionProfilePrompt>();           
            window.Initialize(onComplete);
        }

        private void Initialize(Action<string> onComplete)
        {
            SetWindow();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(viewFilePath);
            visualTree.CloneTree(rootVisualElement);
            BindUIElements();
            RegisterUICallbacks();
            
            this.onComplete = onComplete;

            Show(true);
            Focus();
        }

        private void SetWindow()
        {
            titleContent.text = "Profile name";
            maxSize = new Vector2(400, 250);
            minSize = new Vector2(400, 250);
            Rect main = Utils.GetUnityCurrentMonitorRect();
            float centerWidth = (main.width - minSize.x) * 0.5f;
            float centerHeight = main.height * .33f - minSize.y * 0.5f; // .33 puts dialog in upper third of parent window for aesthetics
            position = new Rect(
                main.x + centerWidth,
                main.y + centerHeight,
                minSize.x,
                minSize.y);

        }

        private void BindUIElements()
        {
            profileNameInput = rootVisualElement.Q<TextField>("profileName-input");
            yesButton = rootVisualElement.Q<Button>("yes-button");
            cancelButton = rootVisualElement.Q<Button>("cancel-button");
        }

        private void RegisterUICallbacks()
        {
            yesButton.clicked += OnYesButtonClicked;
            cancelButton.clicked += OnCancelButtonClicked;
        }
        
        private void OnYesButtonClicked()
        {
            result = profileNameInput.value;
            
            Close();
            onComplete?.Invoke(result);
        }
        
        private void OnCancelButtonClicked()
        {
            Close();
            onComplete?.Invoke(result);
        }
    }
}