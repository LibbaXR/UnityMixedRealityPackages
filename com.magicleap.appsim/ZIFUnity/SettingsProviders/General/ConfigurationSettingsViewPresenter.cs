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
using System.IO;
using ml.zi;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.XR.MagicLeap;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ConfigurationSettingsViewPresenter : CustomView
    {
        public event Action<string> OnDefaultRoomPathChanged;
        public event Action<Settings.DirtySessionState> OnDirtySessionChanged;
        public event Action<bool> OnHardwareAccelerationChanged;

        public event Action<LoggingLevel> OnLogLevelChanged;
        public event Action OnResetButtonClicked;

        private TextField defaultRoomPathField;

        private EnumField dirtySessionField;
        private Button filePickerButton;
        private Toggle hardwareAccelerationToggle;
        private IMGUIContainer helpBoxContainer;
        private EnumField logLevelField;

        private string roomMessageText;
        private MessageType roomMessageType;

        public event Action<bool> OnShowCloseSessionDialogChanged;
        public event Action<bool> OnCloseZiSessionOnQuitChanged;
        private Toggle showCloseSessionDialogToggle;
        private Toggle closeZiSessionOnQuitToggle;

        private Button resetSettingsButton;

        public ConfigurationSettingsViewPresenter(VisualElement root)
        {
            Root = root;
            Initialize();
        }

        public void SetDefaultRoomPath(string modelDefaultRoomPath)
        {
            defaultRoomPathField.SetValueWithoutNotify(modelDefaultRoomPath);
            SetDefaultSessionHelpBox();
        }

        public void SetDirtySessionState(Settings.DirtySessionState modelDirtySessionState)
        {
            dirtySessionField.SetValueWithoutNotify(modelDirtySessionState);
        }

        public void SetHardwareAcceleration(bool modelHardwareAcceleration)
        {
            hardwareAccelerationToggle.SetValueWithoutNotify(modelHardwareAcceleration);
        }

        public void SetCloseSessionDialog(bool closeSessionDialog)
        {
            showCloseSessionDialogToggle.SetValueWithoutNotify(closeSessionDialog);
        }

        public void SetCloseZiSessionOnQuit(bool closeSessionDialog)
        {
            closeZiSessionOnQuitToggle.SetValueWithoutNotify(closeSessionDialog);
        }

        public void SetLogLevel(LoggingLevel modelLogLevel)
        {
            logLevelField.SetValueWithoutNotify(modelLogLevel);
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(logLevelField, nameof(logLevelField));
            Assert.IsNotNull(hardwareAccelerationToggle, nameof(hardwareAccelerationToggle));
            Assert.IsNotNull(defaultRoomPathField, nameof(defaultRoomPathField));
            Assert.IsNotNull(filePickerButton, nameof(filePickerButton));
            Assert.IsNotNull(helpBoxContainer, nameof(helpBoxContainer));
            Assert.IsNotNull(dirtySessionField, nameof(dirtySessionField));
            Assert.IsNotNull(showCloseSessionDialogToggle, nameof(showCloseSessionDialogToggle));
            Assert.IsNotNull(closeZiSessionOnQuitToggle, nameof(closeZiSessionOnQuitToggle));
            Assert.IsNotNull(resetSettingsButton, nameof(resetSettingsButton));
        }

        protected override void BindUIElements()
        {
            logLevelField = Root.Q<EnumField>("log-level");
            hardwareAccelerationToggle = Root.Q<Toggle>("acceleration-toggle");
            defaultRoomPathField = Root.Q<TextField>("room-path");
            filePickerButton = Root.Q<Button>("open-file-button");
            helpBoxContainer = Root.Q<IMGUIContainer>("help-box");
            dirtySessionField = Root.Q<EnumField>("dirty-session-field");
            showCloseSessionDialogToggle = Root.Q<Toggle>("close-dialog-toggle");
            closeZiSessionOnQuitToggle = Root.Q<Toggle>("close-session-on-quit-toggle");
            resetSettingsButton = Root.Q<Button>("reset-settings-button");

            logLevelField.Init(LoggingLevel.Info);
            dirtySessionField.Init(Settings.DirtySessionState.Prompt);
        }

        protected override void RegisterUICallbacks()
        {
            logLevelField.RegisterValueChangedCallback(LogLevelChanged);
            hardwareAccelerationToggle.RegisterValueChangedCallback(HardwareAccelerationChanged);
            defaultRoomPathField.RegisterValueChangedCallback(DefaultRoomPathFieldChanged);
            filePickerButton.clicked += OnFilePickerButtonClicked;
            helpBoxContainer.onGUIHandler += DrawDefaultSessionHelpBox;
            dirtySessionField.RegisterValueChangedCallback(DirtySessionChanged);
            showCloseSessionDialogToggle.RegisterValueChangedCallback(ShowCloseSessionDialogChanged);
            closeZiSessionOnQuitToggle.RegisterValueChangedCallback(CloseZiSessionOnQuitChanged);
            resetSettingsButton.clicked += ResetSettingsButtonClicked;
        }

        protected override void UnregisterUICallbacks()
        {
            logLevelField.UnregisterValueChangedCallback(LogLevelChanged);
            hardwareAccelerationToggle.UnregisterValueChangedCallback(HardwareAccelerationChanged);
            defaultRoomPathField.UnregisterValueChangedCallback(DefaultRoomPathFieldChanged);
            filePickerButton.clicked -= OnFilePickerButtonClicked;
            helpBoxContainer.onGUIHandler -= DrawDefaultSessionHelpBox;
            dirtySessionField.UnregisterValueChangedCallback(DirtySessionChanged);
            showCloseSessionDialogToggle.UnregisterValueChangedCallback(ShowCloseSessionDialogChanged);
            closeZiSessionOnQuitToggle.UnregisterValueChangedCallback(CloseZiSessionOnQuitChanged);
            resetSettingsButton.clicked -= ResetSettingsButtonClicked;
        }

        private void ResetSettingsButtonClicked()
        {
            OnResetButtonClicked?.Invoke();
        }

        private void DefaultRoomPathFieldChanged(ChangeEvent<string> evt)
        {
            OnDefaultRoomPathChanged?.Invoke(evt.newValue);
            SetDefaultSessionHelpBox();
        }

        private void DirtySessionChanged(ChangeEvent<Enum> evt)
        {
            OnDirtySessionChanged?.Invoke((Settings.DirtySessionState) evt.newValue);
        }

        private void DrawDefaultSessionHelpBox()
        {
            EditorGUILayout.HelpBox(roomMessageText, roomMessageType, false);
        }

        private void HardwareAccelerationChanged(ChangeEvent<bool> evt)
        {
            OnHardwareAccelerationChanged?.Invoke(evt.newValue);
        }

        private void ShowCloseSessionDialogChanged(ChangeEvent<bool> evt)
        {
            OnShowCloseSessionDialogChanged?.Invoke(evt.newValue);
        }

        private void CloseZiSessionOnQuitChanged(ChangeEvent<bool> evt)
        {
            OnCloseZiSessionOnQuitChanged?.Invoke(evt.newValue);
        }

        private void LogLevelChanged(ChangeEvent<Enum> evt)
        {
            OnLogLevelChanged?.Invoke((LoggingLevel) evt.newValue);
        }

        private void OnFilePickerButtonClicked()
        {
            string selectedPath = EditorUtility.OpenFilePanel("Select Session or Room file to load", Path.GetDirectoryName(Settings.Instance.DefaultSessionPath), "room,session");
            if (selectedPath.Length != 0)
            {
                defaultRoomPathField.SetValueWithoutNotify(selectedPath);
                OnDefaultRoomPathChanged?.Invoke(selectedPath);
                SetDefaultSessionHelpBox();
            }
        }

        private void SetDefaultSessionHelpBox(string message, MessageType type)
        {
            roomMessageText = message;
            roomMessageType = type;
        }

        private void SetDefaultSessionHelpBox()
        {
            string savedDefaultRoomPath = Path.IsPathFullyQualified(Settings.Instance.DefaultSessionPath) ?
                Settings.Instance.DefaultSessionPath :
                Path.Join(MagicLeapSDKUtil.AppSimRuntimePath, Settings.Instance.DefaultSessionPath);

            if (!string.IsNullOrEmpty(savedDefaultRoomPath))
            {
                if (!File.Exists(savedDefaultRoomPath))
                {
                    SetDefaultSessionHelpBox("No file exists at the given path.", MessageType.Error);
                }
                else
                {
                    bool isRoomPath = savedDefaultRoomPath.EndsWith(".room");
                    bool isSessionPath = savedDefaultRoomPath.EndsWith(".session");

                    if (isRoomPath)
                    {
                        SetDefaultSessionHelpBox("Selected Room: " + Path.GetFileNameWithoutExtension(savedDefaultRoomPath), MessageType.None);
                    }
                    else if(isSessionPath)
                    {
                        SetDefaultSessionHelpBox("Selected Session: " + Path.GetFileNameWithoutExtension(savedDefaultRoomPath), MessageType.None);
                    }
                    else
                    {
                        SetDefaultSessionHelpBox("Path must be to a file with the extension .room or .session", MessageType.Error);
                    }
                }
            }
            else
            {
                SetDefaultSessionHelpBox("No default room or session selected.", MessageType.None);
            }
        }
    }
}
