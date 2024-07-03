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
    internal class SavingDirtyScenePrompt : EditorWindow
    {
        private const int x1dpi = 96;
        
        private string viewFilePath = "Packages/com.magicleap.appsim/ZIFUnity/Editor/SavingDirtyScenePrompt.uxml";
        
        private Label descriptionLabel;
        private Toggle checkboxToggle;
        private Button yesButton;
        private Button noButton;
        private Button cancelButton;

        private bool? result = null;
        private Action<bool?> onComplete;

        public static void ShowPrompt(SessionSaveStatus settings, Action<bool?> onComplete)
        {
            var window = CreateInstance<SavingDirtyScenePrompt>();           
            window.Initialize(settings, onComplete);
        }

        private void Initialize(SessionSaveStatus settings, Action<bool?> onComplete)
        {
            SetWindow();
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(viewFilePath);
            visualTree.CloneTree(rootVisualElement);
            BindUIElements();
            RegisterUICallbacks();

            string path = !string.IsNullOrEmpty(settings.Path) ? settings.Path : "Unsaved Session";
			// change '\' => '/' because unity dialog text line subdivision (for long lines) is confusing by back slashes.
            descriptionLabel.text = string.Format("There are unsaved changes in the room session \"{0}\". Do you want to save it?", path.Replace('\\', '/'));
            this.onComplete = onComplete;
            
            ShowModalUtility();
        }

        private void SetWindow()
        {
            titleContent.text = "Unsaved changes in room session.";
            maxSize = new Vector2(360, 135);
            minSize = new Vector2(360, 135);
            Rect main = Utils.GetUnityCurrentMonitorRect();
            float centerWidth = (main.width - minSize.x) * 0.5f;
            float centerHeight = main.height * .33f - minSize.y * 0.5f;  // .33 puts dialog in upper third of parent window for aesthetics
            position = new Rect(
                main.x + centerWidth,
                main.y + centerHeight,
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
            Close();
            onComplete?.Invoke(result);
        }
        
        private void OnNoButtonClicked()
        {
            if (checkboxToggle.value) 
                Settings.Instance.DirtySessionPrompt = Settings.DirtySessionState.DiscardChanges;
            
            result = false;
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
