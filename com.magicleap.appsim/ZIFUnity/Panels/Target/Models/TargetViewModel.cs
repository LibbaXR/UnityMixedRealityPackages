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
            if (!ZIBridge.instance.IsServerRunning)
            {
                Bridge.StartSessionOnThread(targetMode);
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

        public void ChangeTargetModel(PeripheralTargetMode targetMode)
        {
            if (!Peripheral.IsHandleConnected)
                return;
            
            Peripheral.Handle.SetTargetMode(targetMode);
        }

        public void LoadSession(string sessionFilePath)
        {
            OnStartedLoadingSession?.Invoke();
            AsyncLoadSession(sessionFilePath, (_) => { 
                OnFinishedLoadingSession?.Invoke();
            });
        }

        /// <summary>
        /// Saves the session (if marked dirty) with a confirmation prompt. Returns false if save failed or user cancelled.
        /// Returns true if successful or user declined (chose 'no') to dialog. Also returns true if no save was needed.
        /// </summary>
        /// <param name="onComplete"></param>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        private void ShowSavePrompt(Action<bool> onComplete)
        {
            if (ZIBridge.Instance.SessionSaveStatus.RequiresSave)
            {
                bool? result = null;

                switch (Settings.Instance.DirtySessionPrompt)
                {
                    case Settings.DirtySessionState.Prompt:
                        SavingDirtyScenePrompt.ShowPrompt(ZIBridge.Instance.SessionSaveStatus, (res) =>
                        {
                            if (res.GetValueOrDefault())
                            {
                                ZIBridge.Instance.SaveSessionOnThread(ZIBridge.Instance.SessionSaveStatus.Path, false, onComplete);
                            }
                            else
                            {
                                onComplete?.Invoke(res.HasValue);
                            }
                        });
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

                if (result.GetValueOrDefault())
                {
                    ZIBridge.Instance.SaveSessionOnThread(ZIBridge.Instance.SessionSaveStatus.Path, false, onComplete);
                }
                else if (result.HasValue) // result is null if we executed the Settings.DirtySessionState.Prompt case.
                {
                    onComplete?.Invoke(true);
                }
            }
            else
            {
                onComplete?.Invoke(true);
            }
        }

        private void AsyncLoadSession(string sessionFilePath, Action<bool> onComplete=null)
        {
            ShowSavePrompt((res) => {
                if (res)
                {
                    Bridge.LoadSessionOnThread(sessionFilePath, onComplete);
                }
                else
                {
                    onComplete?.Invoke(false);
                }
            });
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

        public void OnOpenSessionSelected()
        {
            OnStartedPickingSession?.Invoke();
            AsyncOpenSessionSelected((_) => {
                OnFinishedPickingSession?.Invoke();
            });
        }

        private void AsyncOpenSessionSelected(Action<bool> onCompleted)
        {
            ShowSavePrompt((res) => { 
                if (res)
                {
                    string path = EditorUtility.OpenFilePanel("Magic Leap App Simulator - Load Session", GetMostRecentPath(), "session");
                    if (path.Length != 0)
                    {
                        Bridge.LoadSessionOnThread(path, onCompleted);
                    }
                }
                else
                {
                    onCompleted?.Invoke(false);
                }
            });
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
