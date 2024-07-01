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
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using ml.zi;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed partial class ZIBridge
    {
        private static readonly ProgressMonitor tryReconnectMonitor = ProgressMonitorDisplay.Show("Connecting Hand Tracking to session.", "Magic Leap App Simulator");

        public event Action<SessionSaveStatus> OnSessionSaveStatusChanged = null;

        private readonly AsyncTask<bool> asyncSessionStopTask = new("Stopping session", "",
            TaskDisplaySettings.ProgressBackground | TaskDisplaySettings.ProgressWindow);

        private readonly AsyncTask<bool> asyncSessionStartTask = new("Starting session", "",
            TaskDisplaySettings.ProgressBackground | TaskDisplaySettings.ProgressWindow);

        private readonly AsyncTask<bool> asyncSessionSaveTask = new("Saving session", "",
            TaskDisplaySettings.ProgressBackground | TaskDisplaySettings.ProgressWindow);

        private readonly AsyncTask<bool> asyncSessionLoadTask = new("Loading session", "",
            TaskDisplaySettings.ProgressBackground | TaskDisplaySettings.ProgressWindow);

        private Session session = Session.Wrap(0);

        private SessionSaveStatus sessionSaveStatus;
        public SessionSaveStatus SessionSaveStatus => sessionSaveStatus;
        public static bool IsSessionConnected => Instance.IsConnected;
        public static bool IsHandleConnected => Instance.session.GetHandle() != 0 && Instance.session.GetConnected();
        public static SessionTargetMode CurrentTargetMode => Instance.TargetMode;
        public static bool IsSimulatorMode => CurrentTargetMode == SessionTargetMode.Simulator;
        public static bool IsDeviceMode => CurrentTargetMode == SessionTargetMode.Device;

        private const string sessionStatusPathEditorPref = "ZI_SessionStateStatus_Path";
        private const string sessionStatusIsDirtyEditorPref = "ZI_SessionStateStatus_IsDirty";

        private EventQueue zifEventQueue = null;
        private readonly System.Object eventQueueLock = new System.Object();

        private SynchronizationContext syncContext;

        private void InitSession()
        {
            syncContext = System.Threading.SynchronizationContext.Current;
            
            session.SetHandle(GetSessionHandleFromPlugin());
            if (session.GetHandle() != 0)
            {
                ReconnectSession();
            }
        }

        public bool ReconnectSession()
        {
            // This is just a hack because the ZIF API has an "event-ful"
            // API but we don't expect this to (1) take a long time
            // or (2) actually succeed in most cases
            void IgnoreEvent(AsyncEventType type, string message, float percentDone, EventUserMessageType userMessageType)
            {
                // don't log errors when reconnecting fails
            }

            bool result = ReconnectZISession(IgnoreEvent, () => false, out targetMode);
            if (result && targetMode == SessionTargetMode.Unknown)
            {
                TryDisconnectSession();
                reconnectedTimestamp = DateTime.UtcNow;
                result = false;
            }

            TargetMode = targetMode;
            IsConnected = result;
            reconnectedTimestamp = DateTime.UtcNow;
            return result;
        }

        // Extract URL from HTML link tags <a href=...> in the text, 
        // then replace the "<a href=...>" with the URL, namely
        // SRC:    blabla...<a href="http://abc.com">page</a> clacla 
        // TARGET: blabla...http://abc.com clacla
        private string ConvertLinkInMessage(string text)
        {
            Regex pattern = new Regex(".*<a href=\"(?<theURL>.+)\">");
            Match match = pattern.Match(text);
            string url = match.Groups["theURL"].Value;
            if (url != null && url.Length > 0)
            {
                text = Regex.Replace(text, @"<a.+</a>", url);
            }
            return text;
        }

        private bool ValidateGraphics()
        {
            ProgressMonitor monitor = ProgressMonitor.AllocWithTimeout(3000);

            bool hardwareAccelerationCheckboxIsOn = MagicLeap.ZI.Settings.Instance.HardwareAcceleration;

            Result setHardwareAccelerationResult =
                ml.zi.Environment.SetHardwareAccelerationEnabled(hardwareAccelerationCheckboxIsOn);
            if (setHardwareAccelerationResult != Result.Ok) {
              Debug.Log(
                  "Failed to modify hardware acceleration setting state successfully, graphics may still work.");
            }

            ReturnedResultString rrs = ml.zi.Environment.TestGraphicsSupport(monitor);

            if (rrs.first != Result.Ok)
            {
                IsStartingOrStopping = true;
                bool abort = true;
                string msg = rrs.second;

                Func<string, string, bool> displayDialog = (string ok, string cancel) =>
                {
                    // We do this conversion because EditorUtility.DisplayDialog() does not support hyperlink.
                    // Note if the "msg" is too long, it will be auto-truncated by EditorUtility.DisplayDialog().
                    msg = ConvertLinkInMessage(msg);
                    return EditorUtility.DisplayDialog(Constants.zifPluginUserName, msg, ok, cancel);
                };


                // If user selected to turn hardware acceleration off, then don't need to
                // display the dialog.
                if (rrs.first == Result.LowSpeedGraphics )
                {
                    if (hardwareAccelerationCheckboxIsOn) {
                      bool shouldContinue = displayDialog("Continue", "Cancel");
                      abort = !shouldContinue;
                    } else {
                      abort = false;
                    }
                }
                else if (rrs.first == Result.NoGraphicsSupport)
                {
                    // don't give user choice, just abort.
                    displayDialog("Ok", "");
                }
                else
                {
                    // unexpected result. Should not happen.
                    if (msg.Length > 0)
                    {
                        displayDialog("Ok", "");
                    }
                    Debug.LogError("Unexpected result from graphics validation: " + rrs.first);
                }
                if (abort)
                {
                    // This is to make sure the Start/Stop button is re-enabled.
                    IsStartingOrStopping = false;
                    return false;
                }
            }

            return true;
        }

        public void StartSessionOnThread(SessionTargetMode targetMode)
        {
            if (!ValidateGraphics())
            {
                return;
            }
            SubscribeToUserMessageEvents();

            asyncSessionStartTask.Start(StartSessionThread, (result) =>
            {
                if (result)
                    TargetMode = targetMode;
                IsConnected = result;
            });

            void StartSessionThread(AsyncTaskState<bool> taskState)
            {
                try
                {
                    bool sessionConnected = false;

                    IsStartingOrStopping = true;

                    if (StartZISession(targetMode, taskState.EventCallback, () => taskState.Cancel))
                    {
                        sessionConnected = true;
                    }
                    else
                    {
                        StopSessionOnThread();
                    }

                    taskState.Result = sessionConnected;
                }
                catch (ResultIsErrorException e)
                {
                    taskState.Result = false;
                    Debug.LogError(e.Message);
                }
                finally
                {
                    taskState.Complete = true;
                    IsStartingOrStopping = false;
                }
            }
        }

        public void StopSessionOnThread(Action<bool> onComplete = null)
        {
            IsStartingOrStopping = true;

            void OnComplete(bool result)
            {
                TryDisconnectSession();
                if (onComplete != null) 
                { 
                    onComplete(result); 
                }
            }

            StopSessionOnThreadInternal(OnComplete);
        }

        private void StopSessionOnThreadInternal(Action<bool> onComplete)
        {
            UnsubscribeFromUserMessageEvents();

            asyncSessionStopTask.Start(StopSessionThread, onComplete);

            void StopSessionThread(AsyncTaskState<bool> taskState)
            {
                var startState = taskState;

                try
                {
                    startState.Result = StopZISession(startState.EventCallback, () => startState.Cancel);
                }
                catch (ResultIsErrorException e)
                {
                    taskState.Result = false;
                    Debug.LogError(e.Message);
                }
                finally
                {
                    taskState.Complete = true;
                }
            }
        }

        public void SaveSessionOnThread(string path, bool isSaveAs = false, Action<bool> onComplete = null)
        {
            bool isPathNull = string.IsNullOrEmpty(path);

            if (isPathNull || isSaveAs)
            {
                string defaultName = isPathNull ? path : "Untitled.session";

                path = EditorUtility.SaveFilePanel(
                    "Magic Leap App Simulator - Save Session", Settings.Key_DefaultSessionPath, defaultName, "session");

                if (path.Length == 0)
                {
                    onComplete?.Invoke(false);
                    return;
                }
            }

            asyncSessionSaveTask.Start(SaveSessionThread, onComplete);

            void SaveSessionThread(AsyncTaskState<bool> taskState)
            {
                bool isSaveSuccessful = SaveSession(path);

                taskState.Result = isSaveSuccessful;
                taskState.Complete = true;
            }
        }

        public void LoadSessionOnThread(string path, Action<bool> onComplete = null)
        {
            asyncSessionLoadTask.Start(LoadSessionThread, onComplete);

            void LoadSessionThread(AsyncTaskState<bool> taskState)
            {
                bool isLoadSuccessful = LoadSession(path);

                taskState.Result = isLoadSuccessful;
                taskState.Complete = true;
            }
        }

        private void DisconnectModules()
        {
            if (!IsConnected)
            {
                return;
            }

            VirtualRoom.TrySessionDisconnect();
            SceneGraph.TrySessionDisconnect();
            Peripheral.TrySessionDisconnect();
            HeadTrackingHandle.TrySessionDisconnect();
            EyeTrackingHandle.TrySessionDisconnect();
            LeftHandHandle.TrySessionDisconnect();
            RightHandHandle.TrySessionDisconnect();
            ConfigurationSettings.TrySessionDisconnect();
            DeviceConfiguration.TrySessionDisconnect();
            InputController.TrySessionDisconnect();
            ImageTracking.TrySessionDisconnect();
            Permissions.TrySessionDisconnect();
            SystemEvents.TrySessionDisconnect();
            UnsubscribeHandleEvents();
        }

        private void DeviceConfigurationChangesHandler(ConfigurationSettingsChanges configurationSettingsChanges)
        {
            if (configurationSettingsChanges.HasFlag(ConfigurationSettingsChanges.ConfigurationSettingsChanged))
            {
                DeviceConfiguration.Save();
                return;
            }

            if (configurationSettingsChanges.HasFlag(ConfigurationSettingsChanges.DeviceListChanged))
            {
                DeviceConfiguration.Load();
            }
        }

        private void SubscribeToHandleEvents()
        {
            if (DeviceConfiguration.IsHandleConnected && ConfigurationSettings.IsHandleConnected)
            {
                ConfigurationSettings.OnTakeChanges += DeviceConfigurationChangesHandler;
            }
        }

        private void SubscribeToUserMessageEvents()
        {
            lock (eventQueueLock) {
                if (zifEventQueue != null && zifEventQueue.GetSession() == this.session)
                    return;
                if (session != null && session.GetHandle() != 0)
                {
                    if (zifEventQueue != null) { zifEventQueue.Dispose(); }
                    zifEventQueue = EventQueue.Alloc(session);
                    if (zifEventQueue != null)
                    {
                        zifEventQueue.Subscribe(ml.zi.EventType.UserMessage);
                        zifEventQueue.Subscribe(ml.zi.EventType.CancelUserMessage);
                    }
                }
            }
        }

        private void UnsubscribeFromUserMessageEvents()
        {
            lock (eventQueueLock)
            {
                if (zifEventQueue != null)
                {
                    zifEventQueue.Dispose();
                    zifEventQueue = null;
                }
            }
        }

        private void MonitorUserMessageEvents()
        {
            lock (eventQueueLock) {
                if (zifEventQueue != null)
                {
                    bool tookEvent;
                    do
                    {
                        tookEvent = false;
                        ml.zi.Event umEvent = zifEventQueue.TakeNext(ml.zi.EventType.UserMessage);
                        if (umEvent != null)
                        {
                            EventUserMessage umE = EventUserMessage.Wrap(umEvent.GetPointer());
                            syncContext.Post(_ =>
                            {
                                EditorUtility.DisplayDialog(Constants.zifPluginUserName, umE.GetMessage(), "Ok");
                            }, null);
                            Debug.LogFormat("{0}", umE.GetMessage());
                            tookEvent = true;
                        }

                        ml.zi.Event cumEvent = zifEventQueue.TakeNext(ml.zi.EventType.CancelUserMessage);
                        if (cumEvent != null)
                        {
                            EventCancelUserMessage cumE = EventCancelUserMessage.Wrap(cumEvent.GetPointer());
                            Debug.LogFormat("Cancel User Message: {0}", cumE.GetMessageID());
                            tookEvent = true;
                        }
                    } while (tookEvent);
                }

                if (zifEventQueue == null || zifEventQueue.GetSession() != this.session)
                {
                    SubscribeToUserMessageEvents();
                }
            }
        }

        private void UnsubscribeHandleEvents()
        {
            ConfigurationSettings.OnTakeChanges -= DeviceConfigurationChangesHandler;
        }

        private void OnBeforeAssemblyReload()
        {
            //Nothing To do here
        }

        private void OnAfterAssemblyReload()
        {
            //Nothing To do here
        }

        public void ReconnectModules()
        {
            VirtualRoom.ConnectHandle(ml.zi.VirtualRoom.Create(session));
            SceneGraph.ConnectHandle(ml.zi.SceneGraph.Create(VirtualRoom.Handle));
            Peripheral.ConnectHandle(ml.zi.Peripheral.CreateForKeyboardMouse(session));
            HeadTrackingHandle.ConnectHandle(HeadTracking.Create(session));
            EyeTrackingHandle.ConnectHandle(EyeTracking.Create(session));
            LeftHandHandle.ConnectHandle(HandTracking.Create(session, Hand.Left));
            RightHandHandle.ConnectHandle(HandTracking.Create(session, Hand.Right));
            ConfigurationSettings.ConnectHandle(ml.zi.ConfigurationSettings.Create(session));
            DeviceConfiguration.ConnectHandle(ml.zi.DeviceConfiguration.Create(session));
            InputController.ConnectHandle(ml.zi.InputController.Create(session, 0));
            ImageTracking.ConnectHandle(ml.zi.ImageTracking.Create(session));
            Permissions.ConnectHandle(ml.zi.Permissions.Create(session));
            SystemEvents.ConnectHandle(ml.zi.SystemEvents.Create(session));
            SubscribeToHandleEvents();
            SubscribeToUserMessageEvents();
        }

        private void TryDisconnectSession()
        {
            reconnectedTimestamp = DateTime.UtcNow;
            IsStartingOrStopping = true;
            DisconnectModules();

            try
            {
                if (session.GetHandle() != 0)
                {
                    session.Disconnect();
                }
            }
            catch (ResultIsErrorException exception)
            {
                if (exception.Result != Result.SessionNotConnected &&
                    exception.Result != Result.EmptyHandle)
                {
                    Debug.LogWarning($"Session disconnecting failed with result {exception.Result}.");
                }
            }
            finally
            {
                session = Session.Wrap(0);
                IsStartingOrStopping = false;
                IsConnected = false;
            }
        }

        private void UpdateSessionSaveStatus()
        {
            bool currentSaveDirtyState = false;
            string currentSavePath = String.Empty;

            if (VirtualRoom.IsHandleConnected)
            {
                currentSaveDirtyState = VirtualRoom.Handle.GetSessionDirtyState();
                currentSavePath = VirtualRoom.Handle.GetSessionPath();
            }

            if (currentSaveDirtyState != sessionSaveStatus.IsDirty ||
                currentSavePath != sessionSaveStatus.Path)
            {
                sessionSaveStatus.IsDirty = currentSaveDirtyState;
                sessionSaveStatus.Path = currentSavePath;
                OnSessionSaveStatusChanged?.Invoke(sessionSaveStatus);
            }
        }
    }
}
