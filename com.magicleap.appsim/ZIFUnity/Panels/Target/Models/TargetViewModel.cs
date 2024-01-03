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
using System.Threading.Tasks;
using ml.zi;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class TargetViewModel : ViewModel
    {
        public event Action<PeripheralTargetMode> OnPeripheralTargetModeChanged = null;
        public event Action<bool> OnDirtyStateChanged = null;
        public event Action<uint> OnGetControllersResult = null;
        public event Action<string> OnSessionPathChanged = null;
        public event Action<string[]> OnRecentSessionPathsChanged = null;
        public event Action OnStartedPickingSession = null;
        public event Action OnFinishedPickingSession = null;
        public event Action OnStartedLoadingSession = null;
        public event Action OnFinishedLoadingSession = null;

        private readonly string[] recentSessionPaths = new string[maxRecentSessionCount];

        private const int maxRecentSessionCount = 5;

        private const string recentSessionEditorPrefs = "ZI_Recent_Session_";

        private ZIBridge.ModuleWrapper<Peripheral, PeripheralChanges> Peripheral => Bridge.Peripheral;
        public string CurrentSessionSavePath => Bridge.SessionSaveStatus.Path;

        public void ChangeSessionConnection(SessionTargetMode targetMode)
        {
            if (!ZIBridge.IsHandleConnected)
            {
                if (!Bridge.ReconnectSession())
                {
                    Bridge.StartSessionOnThread(targetMode);
                }
            }
            else
            {
                // REM-5916
                // If editor is in play mode, stop it. Otherwise it would take very
                // long time to stop the app sim session.
                if (UnityEditor.EditorApplication.isPlaying)
                {
                    try
                    {
                        UnityEditor.EditorApplication.ExitPlaymode();
                    }
                    catch (Exception e)
                    {
                        // Just log error if any
                        Debug.LogWarning("Failed to exit play mode on stopping App Sim session: " + e.Message);
                    }
                }

                Bridge.StopSessionOnThread();
            }
        }

        private async Task AsyncStopSession()
        {
            var result = await ShowSavePrompt();

            if (!result)
                return;

            await Bridge.AsyncStopSessionOnThread();
        }

        public void ChangeTargetModel(PeripheralTargetMode targetMode)
        {
            if (!Peripheral.IsHandleConnected)
                return;
            
            Peripheral.Handle.SetTargetMode(targetMode);
        }

        public async void LoadSession(string sessionFilePath)
        {
            OnStartedLoadingSession?.Invoke();
            try
            {
                await AsyncLoadSession(sessionFilePath);
            }
            finally
            {
                OnFinishedLoadingSession?.Invoke();
            }
        }

        private async Task<bool> ShowSavePrompt()
        {
            if (ZIBridge.Instance.SessionSaveStatus.RequiresSave)
            {
                bool? result = null;

                switch (Settings.Instance.DirtySessionPrompt)
                {
                    case Settings.DirtySessionState.Prompt:
                        result = await SavingDirtyScenePrompt.AsyncShowPrompt(ZIBridge.Instance.SessionSaveStatus);
                        break;
                    case Settings.DirtySessionState.SaveSession:
                        result = true;
                        break;
                    case Settings.DirtySessionState.DiscardChanges:
                        result = false;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                if (result == null)
                    return false;

                if (result == true)
                {
                    await ZIBridge.Instance.AsyncSaveSessionOnThread(ZIBridge.Instance.SessionSaveStatus.Path);
                }
            }

            return true;
        }

        private async Task AsyncLoadSession(string sessionFilePath)
        {
            var result = await ShowSavePrompt();

            if (!result)
                return;

            await Bridge.AsyncLoadSessionOnThread(sessionFilePath);
        }

        private void OnSessionSaveStatusChanged(SessionSaveStatus saveStatus)
        {
            MoveSessionToMostRecent(saveStatus.Path);
            OnDirtyStateChanged?.Invoke(saveStatus.IsDirty);
            OnSessionPathChanged?.Invoke(saveStatus.Path);
        }

        public void GetControllersQuery()
        {
            OnGetControllersResult(InputController.GetMaxControllers());
        }

        public override void Initialize()
        {
            base.Initialize();
            InitializeRecentSessionList();
            OnSessionSaveStatusChanged(Bridge.SessionSaveStatus);
            ZIBridge.Instance.OnSessionSaveStatusChanged += OnSessionSaveStatusChanged;
        }

        public override void UnInitialize()
        {
            base.UnInitialize();
            ZIBridge.Instance.OnSessionSaveStatusChanged -= OnSessionSaveStatusChanged;
        }

        public async void OnOpenSessionSelected()
        {
            OnStartedPickingSession?.Invoke();
            try
            {
                await AsyncOpenSessionSelected();
            }
            finally
            {
                OnFinishedPickingSession?.Invoke();
            }
        }

        private async Task AsyncOpenSessionSelected()
        {
            var result = await ShowSavePrompt();

            if (!result)
                return;

            string path = EditorUtility.OpenFilePanel("Magic Leap App Simulator - Load Session", GetMostRecentPath(), "session");
            if (path.Length != 0)
            {
                await Bridge.AsyncLoadSessionOnThread(path);
            }
        }

        public void OnSaveSessionAsSelected()
        {
            Bridge.SaveSessionOnThread(CurrentSessionSavePath, isSaveAs: true);
        }

        public void OnSaveSessionSelected()
        {
            if (string.IsNullOrEmpty(CurrentSessionSavePath))
            {
                OnSaveSessionAsSelected();
            }
            else
            {
                Bridge.SaveSessionOnThread(CurrentSessionSavePath);
            }
        }

        public void RemoveSessionFromRecentList(string sessionPath)
        {
            int indexToRemove = Array.IndexOf(recentSessionPaths, sessionPath);
            if (indexToRemove >= 0)
            {
                recentSessionPaths[indexToRemove] = string.Empty;
                EditorPrefs.SetString(recentSessionEditorPrefs + indexToRemove, string.Empty);
            }

            OnRecentSessionPathsChanged?.Invoke(recentSessionPaths);
        }

        protected override bool AreRequiredModulesConnected() => ZIBridge.IsHandleConnected;

        protected override void SessionStarted()
        {
            base.SessionStarted();

            Bridge.ReconnectModules();
            
            OnSessionSaveStatusChanged(Bridge.SessionSaveStatus);

            Peripheral.OnTakeChanges += OnPeripheralChanged;
        }

        protected override void SessionStopped()
        {
            base.SessionStopped();

            Peripheral.OnTakeChanges -= OnPeripheralChanged;
        }

        private void InitializeRecentSessionList()
        {
            for (int i = 0; i < maxRecentSessionCount; i++)
            {
                recentSessionPaths[i] = EditorPrefs.GetString(recentSessionEditorPrefs + i, string.Empty);
            }

            OnRecentSessionPathsChanged?.Invoke(recentSessionPaths);
        }

        private string GetMostRecentPath()
        {
            string mostRecent = Path.GetDirectoryName(Settings.Instance.DefaultSessionPath);
            if (recentSessionPaths.Length > 0 && !string.IsNullOrEmpty(recentSessionPaths[0]))
            {
                mostRecent = Path.GetDirectoryName(recentSessionPaths[0]);
            }

            return mostRecent;
        }

        private void MoveSessionToMostRecent(string newMostRecentPath)
        {
            if (String.IsNullOrEmpty(newMostRecentPath))
            {
                return;
            }

            int firstIndex = 0;
            string mostRecentPath = recentSessionPaths[firstIndex];
            int foundIndex = Array.IndexOf(recentSessionPaths, newMostRecentPath);

            recentSessionPaths[firstIndex] = newMostRecentPath;
            EditorPrefs.SetString(recentSessionEditorPrefs + firstIndex, newMostRecentPath);

            int moveBorder = foundIndex >= 0 ? Mathf.Min(foundIndex + 1, maxRecentSessionCount) : maxRecentSessionCount;

            for (int i = 1; i < moveBorder; i++)
            {
                string lastRecentSessionPath = recentSessionPaths[i];
                recentSessionPaths[i] = mostRecentPath;
                EditorPrefs.SetString(recentSessionEditorPrefs + i, mostRecentPath);
                mostRecentPath = lastRecentSessionPath;
            }

            OnRecentSessionPathsChanged?.Invoke(recentSessionPaths);
        }

        private void OnPeripheralChanged(PeripheralChanges changes)
        {
            if (changes.HasFlag(PeripheralChanges.TargetMode))
            {
                if (!Peripheral.IsHandleConnected)
                    return;
                
                OnPeripheralTargetModeChanged?.Invoke(Peripheral.Handle.GetTargetMode());
            }
        }
    }
}
