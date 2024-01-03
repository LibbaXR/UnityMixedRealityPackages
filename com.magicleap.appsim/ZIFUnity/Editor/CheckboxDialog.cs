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
using NUnit.Framework.Constraints;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class CheckboxDialog : EditorWindow
    {
        private const int x1dpi = 96;

        private event Action<bool> YesButtonClicked;
        private event Action<bool> NoButtonClicked;
        private string viewFilePath = "Packages/com.magicleap.appsim/ZIFUnity/Editor/CheckboxDialogView.uxml";

        private Label descriptionLabel;
        private Toggle checkboxToggle;
        private Button yesButton;
        private Button noButton;

        private bool shown = false;
        
        public static void ShowDialog(CheckboxDialogSettings settings)
        {
            var window = CreateInstance<CheckboxDialog>();           
            window.Initialize(settings);
        }

        public static async Task AsyncShowDialog(CheckboxDialogSettings settings)
        {
            var window = CreateInstance<CheckboxDialog>();           
            window.Initialize(settings);
            
            var waitTask = Task.Run(async () =>
            {
                while (window.shown) await Task.Delay(25);
            });

            if(waitTask != await Task.WhenAny(waitTask))
                throw new TimeoutException();
        }
        
        private void Initialize(CheckboxDialogSettings settings)
        {
            SetWindow(settings);
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(viewFilePath);
            visualTree.CloneTree(rootVisualElement);
            BindUIElements();
            RegisterUICallbacks();
            SetUIElements(settings);          
            ApplyStyles(settings);

            shown = true;
            
            ShowModalUtility();
        }

        private void SetWindow(CheckboxDialogSettings settings)
        {
            titleContent.text = settings.Name;
            maxSize = settings.Size;
            minSize = settings.Size;
            YesButtonClicked += settings.YesAction;
            NoButtonClicked += settings.NoAction;
            position = new Rect(
                (Screen.currentResolution.width * x1dpi / Screen.dpi - settings.Size.x) / 2,
                (Screen.currentResolution.height * x1dpi / Screen.dpi - settings.Size.y) / 2,
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
            shown = false;
            Close();
            YesButtonClicked?.Invoke(checkboxToggle.value);
        }
        private void OnNoButtonClicked()
        {
            shown = false;
            Close();
            NoButtonClicked?.Invoke(checkboxToggle.value);
        }
    }
}
