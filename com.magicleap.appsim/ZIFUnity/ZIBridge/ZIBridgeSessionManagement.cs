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
        public static bool IsHybridMode => CurrentTargetMode == SessionTargetMode.Hybrid;
        public static bool IsDeviceMode => CurrentTargetMode == SessionTargetMode.Device;

        private const string sessionStatusPathEditorPref = "ZI_SessionStateStatus_Path";
        private const string sessionStatusIsDirtyEditorPref = "ZI_SessionStateStatus_IsDirty";

        private bool reportedIncompatibleError = false;
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

        public bool ReconnectSession() => ReconnectSession(out SessionTargetMode targetMode);

        public bool ReconnectSession(out SessionTargetMode targetMode)
        {
            // This is just a hack because the ZIF API has an "event-ful"
            // API but we don't expect this to (1) take a long time
            // or (2) actually succeed in most cases
            void IgnoreEvent(AsyncEventType type, string message, float percentDone, EventUserMessageType userMessageType)
            {
                // don't log errors when reconnecting fails
            }

            bool result = ReconnectZISession(IgnoreEvent, () => false, out targetMode) && TryConnectToTargetMode(targetMode);

            // NOTE: the targetMode logic was never hooked up in ReconnectZISession.
            // Ask ZIF directly.
            try
            {
                var modeAndExact = session.DetectTargetMode();
                targetMode = modeAndExact.first;
            }
            catch (ResultIsErrorException)
            {
                // ignore
                targetMode = SessionTargetMode.Unknown;
            }

            IsConnected = result;
            TargetMode = targetMode;
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
            ReturnedResultString rrs = ml.zi.Environment.TestGraphicsSupport(monitor);
            if (rrs.first != Result.Ok)
            {
                IsStartingOrStopping = true;
                bool abort = true;
                string msg = rrs.second;
                // We do this conversion because EditorUtility.DisplayDialog() does not support hyperlink.
                // Note if the "msg" is too long, it will be auto-truncated by EditorUtility.DisplayDialog().
                msg = ConvertLinkInMessage(msg);

                if (rrs.first == Result.LowSpeedGraphics)
                {
                    // give user choice of abort or go-on
                    bool goon = EditorUtility.DisplayDialog(Constants.zifPluginUserName, msg, "Continue", "Cancel");
                    abort = !goon;
                }
                else if (rrs.first == Result.NoGraphicsSupport)
                {
                    // don't give user choice, just abort.
                    EditorUtility.DisplayDialog(Constants.zifPluginUserName, msg, "Ok");
                }
                else
                {
                    // unexpected result. Should not happen.
                    if (msg.Length > 0)
                    {
                        EditorUtility.DisplayDialog(Constants.zifPluginUserName, msg, "Ok");
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
                IsConnected = result;
                
                if (result)
                    TargetMode = targetMode;
            });

            void StartSessionThread(AsyncTaskState<bool> taskState)
            {
                try
                {
                    bool sessionConnected = false;

                    IsStartingOrStopping = true;

                    if (StartZISession(targetMode, taskState.EventCallback, () => taskState.Cancel))
                    {
                        sessionConnected = TryConnectToTargetMode(targetMode);
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

        public async Task<bool> AsyncStopSessionOnThread()
        {
            IsStartingOrStopping = true;

            bool status = false;
            using (var sph = new SemaphoreSlim(0, 1))
            {
                void OnComplete(bool result)
                {
                    sph.Release();

                    status = result;
                    if (result) TryDisconnectSession();

                    IsStartingOrStopping = false;
                }

                StopSessionOnThreadInternal(OnComplete);

                var t = sph.WaitAsync();

                if (await Task.WhenAny(t, Task.Delay(10000)) == t)
                {
                    return status;
                }
            }
            throw new TimeoutException(); // whatever you want to do here
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

        public async Task<bool> AsyncSaveSessionOnThread(string path)
        {
            bool result = false;
            using (var sph = new SemaphoreSlim(0, 1))
            {
                SaveSessionOnThread(path, b =>
                {
                    result = b;
                    sph.Release();
                });

                var t = sph.WaitAsync();

                if (await Task.WhenAny(t, Task.Delay(10000)) == t)
                {
                    return result;
                }
            }
            throw new TimeoutException(); // whatever you want to do here
        }

        public void SaveSessionOnThread(string path, Action<bool> onComplete = null, bool isSaveAs = false)
        {
            bool isPathNull = string.IsNullOrEmpty(path);

            if (isPathNull || isSaveAs)
            {
                string defaultName = isPathNull ? path : "Untitled.session";

                path = EditorUtility.SaveFilePanel(
                    "Magic Leap App Simulator - Save Session", Settings.Key_DefaultSessionPath, defaultName, "session");

                if (path.Length == 0)
                    throw new Exception("Chosen path is invalid.");
            }

            asyncSessionSaveTask.Start(SaveSessionThread, onComplete);

            void SaveSessionThread(AsyncTaskState<bool> taskState)
            {
                bool isSaveSuccessful = SaveSession(path);

                taskState.Result = isSaveSuccessful;
                taskState.Complete = true;
            }
        }

        public async Task<bool> AsyncLoadSessionOnThread(string path)
        {
            if (string.IsNullOrEmpty(path))
            {
                path = EditorUtility.OpenFilePanel(
                    "Magic Leap App Simulator - Load Session", Settings.Key_DefaultSessionPath, "session");

                if (path.Length == 0)
                    throw new Exception("Chosen path is invalid.");
            }

            bool result = false;
            using (var sph = new SemaphoreSlim(0, 1))
            {
                LoadSessionOnThread(path, b =>
                {
                    result = b;
                    sph.Release();
                });

                var t = sph.WaitAsync();

                if (await Task.WhenAny(t, Task.Delay(10000)) == t)
                {
                    return result;
                }
            }
            throw new TimeoutException(); // whatever you want to do here
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
                if (zifEventQueue != null)
                    return;

                zifEventQueue = EventQueue.Alloc(session);
                zifEventQueue.Subscribe(ml.zi.EventType.UserMessage);
                zifEventQueue.Subscribe(ml.zi.EventType.CancelUserMessage);
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
                if (zifEventQueue == null)
                    return;

                ml.zi.Event umEvent = zifEventQueue.TakeNext(ml.zi.EventType.UserMessage);
                if (umEvent != null) {
                    EventUserMessage umE = EventUserMessage.Wrap(umEvent.GetPointer());
                    syncContext.Post(_=>{
                        EditorUtility.DisplayDialog(Constants.zifPluginUserName, umE.GetMessage(), "Ok");
                    }, null);
                    Debug.LogFormat("{0}", umE.GetMessage());
                }

                ml.zi.Event cumEvent = zifEventQueue.TakeNext(ml.zi.EventType.CancelUserMessage);
                if (cumEvent != null) {
                    EventCancelUserMessage cumE = EventCancelUserMessage.Wrap(cumEvent.GetPointer());
                    Debug.LogFormat("Cancel User Message: {0}", cumE.GetMessageID());
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

        private bool TryConnectToTargetMode(SessionTargetMode sessionTargetMode)
        {
            bool isSessionConnected = false;

            try
            {
                session.SetHandle(GetSessionHandleFromPlugin());
                session.Connect(tryReconnectMonitor);
                isSessionConnected = session.GetConnected();
            }
            catch
            {
                Debug.Log("There is no active session to connect.");
            }

            return isSessionConnected;
        }

        private void CheckSessionConnection()
        {
            // Server is "connected" to this session.  Session.GetConnected() is not guaranteed
            // to re-check server state (since that's slow).
            Result runningResult = Result.DoesNotExist;
            bool isConnected = false;
            if (session.GetHandle() != 0 && session.GetConnected())
            {
                runningResult = session.Server.GetRunningResult();
                isConnected = ZIFGen.ResultIsStatus(runningResult);
                if (runningResult == Result.IncompatibleRuntime)
                {
                    isConnected = false;
                    if (!reportedIncompatibleError) {
                        Debug.LogError("App Sim runtime is not compatible with ZIF.  Please verify the Unity ZIF and App Sim runtime match.");
                        reportedIncompatibleError = true;
                    }
                }
            }

            IsConnected = isConnected;
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
