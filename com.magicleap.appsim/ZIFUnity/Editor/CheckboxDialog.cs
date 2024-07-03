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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

namespace MagicLeap.ZI
{
    internal class CheckboxDialog : EditorWindow
    {
        private const int x1dpi = 96;

        private Action<bool> yesButtonClicked;
        private Action<bool> noButtonClicked;
        private string viewFilePath = "Packages/com.magicleap.appsim/ZIFUnity/Editor/CheckboxDialogView.uxml";

        private Label descriptionLabel;
        private Toggle checkboxToggle;
        private Button yesButton;
        private Button noButton;

        private Action onComplete;


        public static void ShowDialog(CheckboxDialogSettings settings, Action onComplete = null)
        {
            var window = CreateInstance<CheckboxDialog>();           
            window.Initialize(settings, onComplete);
        }

        private void Initialize(CheckboxDialogSettings settings, Action onComplete)
        {
            SetWindow(settings);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(viewFilePath);
            visualTree.CloneTree(rootVisualElement);
            BindUIElements();
            RegisterUICallbacks();
            SetUIElements(settings);          
            ApplyStyles(settings);

            this.onComplete = onComplete;
            
            ShowModalUtility();
        }
        private void SetWindow(CheckboxDialogSettings settings)
        {
            titleContent.text = settings.Name;
            maxSize = settings.Size;
            minSize = settings.Size;
            yesButtonClicked += settings.YesAction;
            noButtonClicked += settings.NoAction;
            Rect main = Utils.GetUnityCurrentMonitorRect();
            float centerWidth = (main.width - settings.Size.x) * 0.5f;
            float centerHeight = main.height *.33f - settings.Size.y * 0.5f;  // .33 puts dialog in upper third of parent window for aesthetics
            position = new Rect(
                main.x + centerWidth,
                main.y + centerHeight,
                settings.Size.x,
                settings.Size.y);
        }

        private void BindUIElements()
        {
            descriptionLabel = rootVisualElement.Q<Label>("DialogMessage");
            checkboxToggle = rootVisualElement.Q<Toggle>("DialogToggle");
            yesButton = rootVisualElement.Q<Button>("YesButton");
            noButton = rootVisualElement.Q<Button>("NoButton");
        }

        private void SetUIElements(CheckboxDialogSettings settings)
        {
            descriptionLabel.text = settings.Description;
            checkboxToggle.label = settings.CheckboxText;
            yesButton.text = settings.YesButtonText;
            noButton.text = settings.NoButtonText;
        }

        private void RegisterUICallbacks()
        {
            yesButton.clicked += OnYesButtonClicked;
            noButton.clicked += OnNoButtonClicked;
        }

        private void ApplyStyles(CheckboxDialogSettings settings)
        {
            if (settings.StyleSheetPaths == null)
                return;
            VisualElementStyleSheetSet styleSheets = rootVisualElement.styleSheets;
            foreach (string styleSheetPath in settings.StyleSheetPaths)
            {
                StyleSheet styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(styleSheetPath);
                if (styleSheet == null)
                {
                    Debug.LogErrorFormat("Couldn't load style sheet in path: {0}", styleSheetPath);
                    continue;
                }               
                bool hasThisStyle = styleSheets.Contains(styleSheet);
                if (!hasThisStyle)
                {
                    styleSheets.Add(styleSheet);
                }
            }
        }

        private void OnYesButtonClicked()
        {
            Close();
            yesButtonClicked?.Invoke(checkboxToggle.value);
            onComplete?.Invoke();
        }
        private void OnNoButtonClicked()
        {
            Close();
            noButtonClicked?.Invoke(checkboxToggle.value);
            onComplete?.Invoke();
        }
    }
}
