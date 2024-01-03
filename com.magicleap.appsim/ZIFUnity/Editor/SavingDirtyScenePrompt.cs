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
    internal class SavingDirtyScenePrompt : EditorWindow
    {
        private const int x1dpi = 96;
        
        private string viewFilePath = "Packages/com.magicleap.appsim/ZIFUnity/Editor/SavingDirtyScenePrompt.uxml";
        
        private Label descriptionLabel;
        private Toggle checkboxToggle;
        private Button yesButton;
        private Button noButton;
        private Button cancelButton;

        private bool shown = false;
        private bool? result = null;
        public static async Task<bool?> AsyncShowPrompt(SessionSaveStatus settings)
        {
            var window = CreateInstance<SavingDirtyScenePrompt>();           
            window.Initialize(settings);
            
            var waitTask = Task.Run(async () =>
            {
                while (window.shown) 
                    await Task.Delay(25);
            });

            if(waitTask != await Task.WhenAny(waitTask))
                throw new TimeoutException();

            return window.result;
        }

        private void Initialize(SessionSaveStatus settings)
        {
            SetWindow();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(viewFilePath);
            visualTree.CloneTree(rootVisualElement);
            BindUIElements();
            RegisterUICallbacks();

            string path = !string.IsNullOrEmpty(settings.Path) ? settings.Path : "Unsaved Session";
            descriptionLabel.text = $"There are unsaved changes in the room session \"{path}\". Do you want to save it? ";
            shown = true;
            
            ShowModalUtility();
        }

        private void SetWindow()
        {
            titleContent.text = "Unsaved changes in room session.";
            maxSize = new Vector2(360, 135);
            minSize = new Vector2(360, 135);
            position = new Rect(
                (Screen.currentResolution.width * x1dpi / Screen.dpi - minSize.x) / 2,
                (Screen.currentResolution.height * x1dpi / Screen.dpi - minSize.y) / 2,
                minSize.x,
                minSize.y);
        }

        private void BindUIElements()
        {
            descriptionLabel = rootVisualElement.Q<Label>("DialogMessage");
            checkboxToggle = rootVisualElement.Q<Toggle>("DialogToggle");
            yesButton = rootVisualElement.Q<Button>("YesButton");
            noButton = rootVisualElement.Q<Button>("NoButton");
            cancelButton = rootVisualElement.Q<Button>("CancelButton");
        }

        private void RegisterUICallbacks()
        {
            yesButton.clicked += OnYesButtonClicked;
            noButton.clicked += OnNoButtonClicked;
            cancelButton.clicked += OnCancelButtonClicked;
        }
        
        private void OnYesButtonClicked()
        {
            if (checkboxToggle.value) 
                Settings.Instance.DirtySessionPrompt = Settings.DirtySessionState.SaveSession;

            result = true;
            shown = false;
            Close();
        }
        
        private void OnNoButtonClicked()
        {
            if (checkboxToggle.value) 
                Settings.Instance.DirtySessionPrompt = Settings.DirtySessionState.DiscardChanges;
            
            result = false;
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
