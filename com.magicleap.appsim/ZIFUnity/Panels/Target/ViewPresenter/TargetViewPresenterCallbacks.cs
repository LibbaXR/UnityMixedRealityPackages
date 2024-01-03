// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using ml.zi;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed partial class TargetViewPresenter
    {
        public event Action<string> OnRecentSessionChanged;
        public event Action<SessionTargetMode> OnConnectButtonClicked;
        public event Action OnGetControllersQuery;
        public event Action OnOpenSessionSelected;
        public event Action OnSaveSessionAsSelected;
        public event Action OnSaveSessionSelected; 
        public event Action<string> OnRemoveSessionFromRecentListSelected;

        public void OnConnectionSuccess()
        {
            targetConnectButton.tooltip = "Stop App Sim";
            targetConnectButton.SetEnabled(true);
            
            if (startStopIcon.ClassListContains("startTargetIcon"))
            {
                startStopIcon.RemoveFromClassList("startTargetIcon");
                startStopIcon.AddToClassList("stopTargetIcon");
            }

            targetCameraButton.SetEnabled(true);
            sessionDropdownButton.SetEnabled(true);
            
            if (currentSessionLabel.ClassListContains("disabledLabel"))
            {
                currentSessionLabel.RemoveFromClassList("disabledLabel");
            }
            
            targetEnumField.SetEnabled(false);
            if (ZIBridge.CurrentTargetMode != SessionTargetMode.Unknown)
                targetEnumField.SetValueWithoutNotify(currentTargetMode = (TargetViewState.SelectableTargets)ZIBridge.CurrentTargetMode);
        }

        public void OnDisconnect()
        {
            targetConnectButton.tooltip = "Start App Sim";
            targetConnectButton.SetEnabled(true);
            if (startStopIcon.ClassListContains("stopTargetIcon"))
            {
                startStopIcon.RemoveFromClassList("stopTargetIcon");
                startStopIcon.AddToClassList("startTargetIcon");
            }

            targetCameraButton.SetEnabled(false);
            sessionDropdownButton.SetEnabled(false);
            
            if (!currentSessionLabel.ClassListContains("disabledLabel"))
            {
                currentSessionLabel.AddToClassList("disabledLabel");
            }
            
            targetEnumField.SetEnabled(true);
            if (ZIBridge.CurrentTargetMode != SessionTargetMode.Unknown)
                targetEnumField.SetValueWithoutNotify(currentTargetMode = (TargetViewState.SelectableTargets)ZIBridge.CurrentTargetMode);
        }

        public void SetEnabled(bool enabled)
        {
            Root.SetEnabled(enabled);
        }
        
        protected override void RegisterUICallbacks()
        {
            targetEnumField.RegisterValueChangedCallback(SessionTypeClicked);
            targetConnectButton.clicked += OnConnectButtonClickedInternal;
            targetCameraButton.clicked += OnModeMenuClicked;
            sessionDropdownButton.clicked += OnSessionMenuClicked;
        }

        protected override void UnregisterUICallbacks()
        {
            targetEnumField.UnregisterValueChangedCallback(SessionTypeClicked);
            targetConnectButton.clicked -= OnConnectButtonClickedInternal;
            targetCameraButton.clicked -= OnModeMenuClicked;
            sessionDropdownButton.clicked -= OnSessionMenuClicked;
        }

        private void OnConnectButtonClickedInternal()
        {
            targetConnectButton.SetEnabled(false);
            var runningOrNewMode = (SessionTargetMode) State.TargetMode;
            OnConnectButtonClicked(runningOrNewMode);
            if (runningOrNewMode != SessionTargetMode.Unknown)
            {
                State.TargetMode = (TargetViewState.SelectableTargets) runningOrNewMode;
            }
        }

        private void OnModeMenuClicked()
        {
            modeMenu.ShowAsContext();
        }

        private void OnSessionMenuClicked()
        {
            sessionMenu.ShowAsContext();
        }

        private void SessionTypeClicked(ChangeEvent<Enum> evt)
        {
            currentTargetMode = (TargetViewState.SelectableTargets) evt.newValue;
            State.TargetMode = currentTargetMode;
        }
    }
}
