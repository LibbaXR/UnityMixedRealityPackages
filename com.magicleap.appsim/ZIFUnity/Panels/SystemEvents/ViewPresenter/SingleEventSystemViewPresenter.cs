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
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class SingleEventSystemViewPresenter : CustomView
    {
        public event Action<bool> FoldoutChanged;
        public event Action<string> EventValueChanged;
        public event Action<string> ParameterValueChanged;
        public event Action<string> FireClicked;
        public event Action<string> CancelClicked;

        private Foldout foldout;
        private DropdownField eventDropdown;
        private Label infoLabel;
        private VisualElement warningObject;
        private VisualElement parametersContainer;
        private VisualElement loadingImage;
        private Label warningLabel;
        private Button fireButton;
        private Button cancelButton;

        public SingleEventSystemViewPresenter(VisualElement root)
        {
            VisualTreeAsset visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.magicleap.appsim/ZIFUnity/Panels/SystemEvents/Views/SingleSystemEventView.uxml");
            TemplateContainer template = visualTree.Instantiate();

            root.hierarchy.Add(template);
            base.Root = template.contentContainer;

            base.Initialize();

            infoLabel.SetDisplay(false);
            warningObject.SetDisplay(false);
            SetFireButtonEnabled(eventDropdown.value);
            loadingImage.SetDisplay(false);
        }

        protected override void BindUIElements()
        {
            foldout = Root.Q<Foldout>("foldout");
            eventDropdown = Root.Q<DropdownField>("event-dropdown");
            infoLabel = Root.Q<Label>("info-label");
            warningObject = Root.Q<VisualElement>("warning-object");
            parametersContainer = Root.Q<VisualElement>("parameters-container");
            loadingImage = Root.Q<VisualElement>("loading-image");
            warningLabel = Root.Q<Label>("warning-label");
            fireButton = Root.Q<Button>("fire-button");
            cancelButton = Root.Q<Button>("cancel-button");
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(foldout, nameof(foldout));
            Assert.IsNotNull(eventDropdown, nameof(eventDropdown));
            Assert.IsNotNull(infoLabel, nameof(infoLabel));
            Assert.IsNotNull(warningObject, nameof(warningObject));
            Assert.IsNotNull(warningLabel, nameof(warningLabel));
            Assert.IsNotNull(fireButton, nameof(fireButton));
            Assert.IsNotNull(cancelButton, nameof(cancelButton));
            Assert.IsNotNull(parametersContainer, nameof(parametersContainer));
            Assert.IsNotNull(loadingImage, nameof(loadingImage));
        }

        protected override void RegisterUICallbacks()
        {
            foldout.RegisterValueChangedCallback(OnFoldoutChanged);
            eventDropdown.RegisterValueChangedCallback(OnEventValueChanged);
            fireButton.clicked += OnFire;
            cancelButton.clicked += OnCancel;
            EventValueChanged += SetFireButtonEnabled;
        }

        private void OnFoldoutChanged(ChangeEvent<bool> evt) => FoldoutChanged?.Invoke(evt.newValue);
        private void OnEventValueChanged(ChangeEvent<string> evt) => EventValueChanged?.Invoke(evt.newValue);
        private void OnParameterValueChanged(ChangeEvent<string> evt) => ParameterValueChanged?.Invoke(evt.newValue);
        private void OnFire()
        {
            StartLoadingAnimation();
            FireClicked?.Invoke(eventDropdown.value);
        }
        private void OnCancel() => CancelClicked?.Invoke(eventDropdown.value);

        public void SetFoldout(bool isOpened, string name)
        {
            foldout.SetValueWithoutNotify(isOpened);
            foldout.text = name;
        }

        public void SetEventList(List<string> eventNames)
        {
            eventDropdown.choices = eventNames;
        }

        public void ClearParameters()
        {
            parametersContainer.Clear();
        }

        public List<KeyValuePair<string, string>> GetParameters()
        {
            List<KeyValuePair<string, string>> pairs = new List<KeyValuePair<string, string>>();

            foreach (DropdownField dropdown in parametersContainer.Children())
                pairs.Add(new KeyValuePair<string, string>(dropdown.label, dropdown.value));

            return pairs;
        }

        public void AddParameter(string name, List<string> parameterValues, Action<string> onValueChange)
        {
            DropdownField newField = new DropdownField();
            newField.label = name;
            newField.choices = parameterValues;
            newField.RegisterValueChangedCallback(e => onValueChange?.Invoke(e.newValue));
            newField.index = 0;

            parametersContainer.Add(newField);
        }

        public void SetDescription(string description)
        {
            infoLabel.SetDisplay(!string.IsNullOrEmpty(description));
            infoLabel.text = description;
        }

        public void SetWarningDescription(string getEventWarningLabel)
        {
            warningObject.SetDisplay(!string.IsNullOrEmpty(getEventWarningLabel));
            warningLabel.text = getEventWarningLabel;
        }

        public void SetFireButtonEnabled(bool enabled)
        {
            fireButton.SetEnabled(enabled);
        }

        public void StopLoadingAnimation()
        {
            fireButton.SetEnabled(true);
            loadingImage.SetDisplay(false);
            loadingImage.schedule.Execute(Rotate).Pause();
        }

        private void StartLoadingAnimation()
        {
            fireButton.SetEnabled(false);
            loadingImage.SetDisplay(true);
            loadingImage.schedule.Execute(Rotate).Every(1000);
        }

        private void SetFireButtonEnabled(string dropdownValue)
        {
            fireButton.SetEnabled(!String.IsNullOrEmpty(dropdownValue));
        }

        private void Rotate()
        {
            loadingImage.experimental.animation.Start(0f, 1f, 1000, OnAnimationChanged);
        }

        private void OnAnimationChanged(VisualElement element, float status)
        {
            element.transform.rotation = Quaternion.Euler(0f, 0f, 360f * status);
        }
    }
}
