// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;
using System;
using UnityEditor;
using UnityEditor.XR.MagicLeap;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class TargetViewController : ViewController<TargetViewModel, TargetViewPresenter>
    {
        private const string InvalidPathMessage = "MLSDK or App Sim Runtime path is not detected. Please check the settings in Preferences > External Tools > Magic Leap";
        private static readonly string windowName = "App Sim Target";
        
        public static bool AskedToImportThisSession = false;

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Target #F1", false, MenuItemPriority_Target)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Target", isValidateFunction: false, priority: MenuItemPriority_Target)]
#endif
        public static void ShowWindow()
        {
            TargetViewController window = GetWindow<TargetViewController>(windowName);
            window.minSize = new Vector2(220, 33);
        }

        private void OnDisable()
        {
            UnregisterCallbacksFromModel();
            UnregisterCallbacksFromPresenter();

            Presenter.OnDisable();
        }

        protected override void Initialize()
        {
            RegisterCallbacksFromModel();
            RegisterCallbacksFromPresenter();
            Presenter.OnEnable(rootVisualElement);

            if (Model.IsSessionRunning)
            {
                Presenter.OnConnectionSuccess();
            }
            else
            {
                Presenter.OnDisconnect();
            }
            
            base.Initialize();

            CheckAppSimPaths();
        }

        protected override void Update()
        {
            base.Update();

            Presenter.Root.SetEnabled(!MagicLeapSDKUtil.SearchingForZI);
        }

        private void OnConnectButtonClicked(SessionTargetMode targetMode)
        {
            Model.ChangeSessionConnection(targetMode);
        }

        private void OnGetControllersQuery()
        {
            Model.GetControllersQuery();
        }

        private void OnGetControllersResult(uint controllers)
        {
            Presenter.UpdateModeMenu(controllers);
        }

        private void OnModelSessionDirtyStateChanged(bool isDirty)
        {
            Presenter.UpdateSessionDirtyIndicator(isDirty, !string.IsNullOrEmpty(Model.CurrentSessionSavePath));
        }

        private void OnModelTargetModeChanged(PeripheralTargetMode sessionTargetMode)
        {
            Presenter.SelectTargetModeWithoutNotify(sessionTargetMode);
        }

        private void OnSessionStarted()
        {
            Presenter.OnConnectionSuccess();
        }

        private void OnSessionStopped()
        {
            Presenter.OnDisconnect();
        }

        private void OnSessionPathChanged(string currentSessionSavePath)
        {
            Presenter.UpdateSessionLabel(currentSessionSavePath);
        }

        private void OnRecentSessionPathsChanged(string[] recentSessionPaths)
        {
            Presenter.UpdateSessionMenu(recentSessionPaths);
        }

        private void OnStartedPickingSession() => Presenter.SetEnabled(false);
        private void OnFinishedPickingSession() => Presenter.SetEnabled(true);

        private void OnStartedLoadingSession() => Presenter.SetEnabled(false);
        private void OnFinishedLoadingSession() => Presenter.SetEnabled(true);

        private void OnMagicLeapSDKPathChanged() => CheckAppSimPaths();
        private void OnEnvironmentPathChanged() => CheckAppSimPaths();

        private void CheckAppSimPaths()
        {
            bool isInvalidPath = !(ZIBridge.luminSdkPathProvider.IsValid && ZIBridge.environmentPathProvider.IsValid);
            Presenter.SetErrorMessage(isInvalidPath, isInvalidPath ? InvalidPathMessage : string.Empty);
        }

        private void OnStartingOrStopping(bool actionStarted)
        {
            Presenter.OnSessionStartingOrStopping(actionStarted);
        }
        private void OnServerRunningChanged(bool serverRunning)
        {
            Presenter.OnServerRunningChanged(serverRunning);
        }

        private void RegisterCallbacksFromModel()
        {
            ZIBridge.Instance.OnStartingOrStoppingChanged += OnStartingOrStopping;
            ZIBridge.Instance.OnServerRunningChanged += OnServerRunningChanged;
            ZIBridge.luminSdkPathProvider.OnMagicLeapSDKPathChanged += OnMagicLeapSDKPathChanged;
            ZIBridge.environmentPathProvider.OnEnvironmentPathChanged += OnEnvironmentPathChanged;
            Model.OnGetControllersResult += OnGetControllersResult;
            Model.OnSessionStarted += OnSessionStarted;
            Model.OnSessionStopped += OnSessionStopped;
            Model.OnRecentSessionPathsChanged += OnRecentSessionPathsChanged;
            Model.OnSessionPathChanged += OnSessionPathChanged;
            Model.OnDirtyStateChanged += OnModelSessionDirtyStateChanged;
            Model.OnPeripheralTargetModeChanged += OnModelTargetModeChanged;
            Model.OnStartedPickingSession += OnStartedPickingSession;
            Model.OnFinishedPickingSession += OnFinishedPickingSession;
            Model.OnStartedLoadingSession += OnStartedLoadingSession;
            Model.OnFinishedLoadingSession += OnFinishedLoadingSession;
        }

        private void RegisterCallbacksFromPresenter()
        {
            Presenter.OnGetControllersQuery += OnGetControllersQuery;
            Presenter.OnConnectButtonClicked += OnConnectButtonClicked;
            Presenter.OnRecentSessionChanged += Model.LoadSession;
            Presenter.OnRemoveSessionFromRecentListSelected += Model.RemoveSessionFromRecentList;
            Presenter.OnOpenSessionSelected += Model.OnOpenSessionSelected;
            Presenter.OnSaveSessionSelected += Model.OnSaveSessionSelected;
            Presenter.OnSaveSessionAsSelected += Model.OnSaveSessionAsSelected;
            Presenter.OnTargetModeChanged += Model.ChangeTargetModel;
        }

        private void UnregisterCallbacksFromModel()
        {
            ZIBridge.Instance.OnStartingOrStoppingChanged -= OnStartingOrStopping;
            ZIBridge.Instance.OnServerRunningChanged -= OnServerRunningChanged;
            ZIBridge.luminSdkPathProvider.OnMagicLeapSDKPathChanged -= OnMagicLeapSDKPathChanged;
            ZIBridge.environmentPathProvider.OnEnvironmentPathChanged -= OnEnvironmentPathChanged;
            Model.OnGetControllersResult -= OnGetControllersResult;
            Model.OnSessionStarted -= OnSessionStarted;
            Model.OnSessionStopped -= OnSessionStopped;
            Model.OnRecentSessionPathsChanged -= OnRecentSessionPathsChanged;
            Model.OnSessionPathChanged -= OnSessionPathChanged;
            Model.OnDirtyStateChanged -= OnModelSessionDirtyStateChanged;
            Model.OnPeripheralTargetModeChanged -= OnModelTargetModeChanged;
            Model.OnStartedPickingSession -= OnStartedPickingSession;
            Model.OnFinishedPickingSession -= OnFinishedPickingSession;
            Model.OnStartedLoadingSession -= OnStartedLoadingSession;
            Model.OnFinishedLoadingSession -= OnFinishedLoadingSession;
        }

        private void UnregisterCallbacksFromPresenter()
        {
            Presenter.OnGetControllersQuery -= OnGetControllersQuery;
            Presenter.OnConnectButtonClicked -= OnConnectButtonClicked;
            Presenter.OnRecentSessionChanged -= Model.LoadSession;
            Presenter.OnRemoveSessionFromRecentListSelected -= Model.RemoveSessionFromRecentList;
            Presenter.OnOpenSessionSelected -= Model.OnOpenSessionSelected;
            Presenter.OnSaveSessionSelected -= Model.OnSaveSessionSelected;
            Presenter.OnSaveSessionAsSelected -= Model.OnSaveSessionAsSelected;
            Presenter.OnTargetModeChanged -= Model.ChangeTargetModel;
        }
    }
}
