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
    internal class SavingPermissionProfilePrompt : EditorWindow
    {
        private const int x1dpi = 96;
        
        private string viewFilePath = "Packages/com.magicleap.appsim/ZIFUnity/Editor/SavingPermissionProfilePrompt.uxml";
        
        private TextField profileNameInput;
        private Button yesButton;
        private Button cancelButton;

        private bool shown = false;
        private string result = null;
        
        public static async Task<string> AsyncShowPrompt()
        {
            var window = CreateInstance<SavingPermissionProfilePrompt>();           
            window.Initialize();
            
            var waitTask = Task.Run(async () =>
            {
                while (window.shown) 
                    await Task.Delay(25);
            });

            if(waitTask != await Task.WhenAny(waitTask))
                throw new TimeoutException();

            return window.result;
        }

        private void Initialize()
        {
            SetWindow();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(viewFilePath);
            visualTree.CloneTree(rootVisualElement);
            BindUIElements();
            RegisterUICallbacks();
            
            shown = true;

            Show(true);
            Focus();
        }

        private void SetWindow()
        {
            titleContent.text = "Profile name";
            maxSize = new Vector2(400, 250);
            minSize = new Vector2(400, 250);
            position = new Rect(
                (Screen.currentResolution.width * x1dpi / Screen.dpi - minSize.x) / 2,
                (Screen.currentResolution.height * x1dpi / Screen.dpi - minSize.y) / 2,
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
            
            shown = false;
            Close();
        }
        
        private void OnCancelButtonClicked()
        {
            
            shown = false;
            Close();
        }
    }
}