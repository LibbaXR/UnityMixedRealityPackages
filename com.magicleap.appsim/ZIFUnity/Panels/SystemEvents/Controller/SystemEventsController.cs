// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal partial class SystemEventsController : ViewController<SystemEventsModel, SystemEventsViewPresenter>
    {
        private const string WindowName = "App Sim System Events";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim System Events #F11", false, MenuItemPriority_SystemEvents)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim System Events", isValidateFunction: false, priority: MenuItemPriority_SystemEvents)]
#endif
        public static void ShowWindow()
        {
            GetWindow<SystemEventsController>(WindowName);
        }

        protected override void Initialize()
        {
            Presenter.EventValueChanged += OnEventValueChanged;
            Presenter.FireClicked += OnFireClicked;
            Presenter.OnEnable(rootVisualElement);
            Presenter.SetPanelActive(Model.IsSessionRunning);
            
            Model.OnSessionStarted += OnSessionConnected;
            Model.OnSessionStopped += OnSessionDisconnected;
            Model.OnEventFinished += Presenter.OnEventFinished;
            base.Initialize();

        }

        private void OnDisable()
        {
            Presenter.EventValueChanged -= OnEventValueChanged;
            Presenter.FireClicked -= OnFireClicked;
            Presenter.OnDisable();
            
            Presenter.SetPanelActive(false);
            
            Model.OnSessionStarted -= OnSessionConnected;
            Model.OnSessionStopped -= OnSessionDisconnected;
            Model.OnEventFinished -= Presenter.OnEventFinished;
            Model.UnInitialize();
        }

        private void OnSessionConnected()
        {
            CreateSystemEvents();
            Presenter.SetPanelActive(true);
        }

        private void OnSessionDisconnected()
        {
            Presenter.SetPanelActive(false);
        }


        private void OnFireClicked(string eventName, string currentEventValue)
        {
            try
            {
                Model.FireEvent(eventName, currentEventValue, Presenter.GetEventParameters(eventName));
            }
            catch (ml.zi.ResultIsErrorException e)
            {
                Debug.LogError(e.Message);
            }

        }

        private void OnEventValueChanged(string systemEventName, string eventName)
        {
            string description = Model.GetEventDescription(systemEventName, eventName);
            Presenter.SetEventDescription(systemEventName, description);
            Presenter.SetWarningLabel(systemEventName, Model.GetEventWarningLabel(eventName));

            Presenter.ClearParameters(systemEventName);
            Dictionary<string, EventParameter> parameters = Model.GetParameters(systemEventName, eventName);

            foreach (var parameter in parameters.Values)
            {
                Presenter.AddNewEventParameter(systemEventName, parameter.Name, parameter.Values);
            }
        }

        private void CreateSystemEvents()
        {
            foreach (SystemEventSet eventSet in Model.SystemEventsList.Values)
            {
                Presenter.AddNewSystemEvent(eventSet.Label, eventSet.Events.Values.Select(e => e.Label).ToList());
            }
        }
    }
}
