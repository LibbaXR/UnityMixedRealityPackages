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
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class SystemEventsViewPresenter : ViewPresenter<SystemEventsViewState>
    {
        public event Action<string, string> EventValueChanged;
        public event Action<string, string> ParameterValueChanged;
        public event Action<string, string> FireClicked;
        public event Action<string, string> CancelClicked;

        private VisualElement systemEventsContainer;
        private readonly Dictionary<string, SingleEventSystemViewPresenter> eventSystems = new();

        public override void OnEnable(VisualElement root)
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.magicleap.appsim/ZIFUnity/Panels/SystemEvents/Views/SystemEventsView.uxml");
            visualTree.CloneTree(root);
            eventSystems.Clear();

            base.OnEnable(root);
        }

        protected override void BindUIElements()
        {
            systemEventsContainer = Root.Q("unity-content-container");
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(systemEventsContainer, nameof(systemEventsContainer));
        }

        public List<KeyValuePair<string, string>> GetEventParameters(string eventName)
        {
            return eventSystems[eventName].GetParameters();
        }

        public void SetEventDescription(string eventName, string description)
        {
            eventSystems[eventName].SetDescription(description);
        }

        public void SetWarningLabel(string eventName, string getEventWarningLabel)
        {
            eventSystems[eventName].SetWarningDescription(getEventWarningLabel);
            if (!String.IsNullOrEmpty(getEventWarningLabel))
            {
                eventSystems[eventName].SetFireButtonEnabled(false);
            }
        }

        public void SetPanelActive(bool isEnabled)
        {
            foreach (var element in Root.Children())
            {
                element.SetEnabled(isEnabled);
            }
        }

        public void AddNewSystemEvent(string name, List<string> eventsList)
        {
            if (eventSystems.ContainsKey(name))
                return;
            
            var eventSystemPresenter = new SingleEventSystemViewPresenter(systemEventsContainer);
            eventSystems.Add(name, eventSystemPresenter);

            eventSystemPresenter.SetFoldout(State.systemEventsOpenedFoldouts.Contains(name), name);
            eventSystemPresenter.SetEventList(eventsList);

            eventSystemPresenter.CancelClicked += e => CancelClicked?.Invoke(name, e);
            eventSystemPresenter.FireClicked += e => FireClicked?.Invoke(name, e);
            eventSystemPresenter.FoldoutChanged += (isOpened) => OnFoldoutChanged(name, isOpened);
            eventSystemPresenter.EventValueChanged += (eventName) => EventValueChanged?.Invoke(name, eventName);
            eventSystemPresenter.ParameterValueChanged += (parameterName) => ParameterValueChanged?.Invoke(name, parameterName);
        }

        public void ClearParameters(string systemEventName)
        {
            eventSystems[systemEventName].ClearParameters();
        }

        public void AddNewEventParameter(string systemEventName, string parameterName, List<string> parametersValues)
        {
            eventSystems[systemEventName].AddParameter(parameterName, parametersValues,
                (newValue) => ParameterValueChanged?.Invoke(systemEventName, newValue));
        }

        public void OnEventFinished(string systemEventName) => eventSystems[systemEventName].StopLoadingAnimation();
        private void OnFoldoutChanged(string eventLabel, bool isOpened)
        {
            if (isOpened && !State.systemEventsOpenedFoldouts.Contains(eventLabel))
            {
                State.systemEventsOpenedFoldouts.Add(eventLabel);
            }
            else if (!isOpened && State.systemEventsOpenedFoldouts.Contains(eventLabel))
            {
                State.systemEventsOpenedFoldouts.Remove(eventLabel);
            }
        }
    }
}
