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
using System.Collections;
using System.Threading;
using ml.zi;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
  internal class AsyncTask<T>
  {
    private readonly IAsyncTaskDisplay display;
    private readonly ProgressMonitor progressMonitor;
    private readonly AsyncProgressMonitor<T> asyncProgressMonitor;

    private AsyncTaskState<T> asyncState;
    private bool isCompleted = true;
    private Thread sessionThread;

    public AsyncTask(string taskName, string taskDescription, TaskDisplaySettings displayType = TaskDisplaySettings.None)
    {
      display = new CompositeTaskDisplay(displayType, taskName, taskDescription, Cancel);
      asyncProgressMonitor = new AsyncProgressMonitor<T>(taskDescription);
      progressMonitor = ProgressMonitor.Alloc(asyncProgressMonitor);
    }

    ~AsyncTask()
    {
      Finish();
    }

    private void OnPlayModeStateChanged(PlayModeStateChange state)
    {
      if (EditorSettings.enterPlayModeOptionsEnabled)
      {
        if ((EditorSettings.enterPlayModeOptions & EnterPlayModeOptions.DisableDomainReload) != 0)
        {
          // user has already turned this option off, so, nothing will interrupt us
          return;
        }
      }

      if (state == PlayModeStateChange.ExitingEditMode)
      {
        UnityEngine.Debug.LogError("App Sim is busy with a task and cannot enter Play mode now.\n" +
            "Please wait to play your scene until it finishes.\n" +
            "Alternately, in Project Settings > Project > Editor > Enter Play Mode Options, enable options and turn off Enable Domain Reload.");
        EditorApplication.isPlaying = false;
      }
    }

    private void Finish()
    {
      if (!isCompleted)
      {
        EditorApplication.playModeStateChanged -= OnPlayModeStateChanged;

        display.Clear();
        if (sessionThread != null)
        {
          sessionThread.Join();
        }
        sessionThread = null;

        isCompleted = true;
      }
    }

    public void Start(Action<AsyncTaskState<T>> threadTask, Action<T> onCompleted = null)
    {
      if (!isCompleted)
      {
        return;
      }

      EditorApplication.playModeStateChanged += OnPlayModeStateChanged;

      EditorCoroutineUtility.StartCoroutine(ProcessSessionStart(), this);

      IEnumerator ProcessSessionStart()
      {
        asyncState = new AsyncTaskState<T>();
        isCompleted = false;
        display.Start();

        sessionThread = new Thread(taskState => threadTask.Invoke(taskState as AsyncTaskState<T>));
        sessionThread.Start(asyncState);

        var waitForEndOfFrame = new WaitForEndOfFrame();

        try
        {
          while (!asyncState.Complete && !asyncState.Cancel)
          {
            ProcessEvents();
            yield return waitForEndOfFrame;
          }
        }
        finally
        {
          Finish();
          onCompleted?.Invoke(asyncState.Result);
        }
      }
    }

    public void StartMonitored(Action<ProgressMonitor> threadTask, Action<T> onCompleted = null)
    {
      Start(asyncState =>
      {
        asyncProgressMonitor.taskState = asyncState;
        threadTask.Invoke(progressMonitor);
      }, onCompleted);
    }

    private void DisplayUserMessage(AsyncEvent asyncEvent)
    {
      string noNewlinesMessage = asyncEvent.Message.Replace('\n', ' ');
      switch (asyncEvent.UserMessageType)
      {
        case EventUserMessageType.Debug:
        case EventUserMessageType.Trace:
        case EventUserMessageType.Unknown:
          Debug.Log(noNewlinesMessage);
          break;
        case EventUserMessageType.Error:
          Debug.LogError(noNewlinesMessage);
          break;
        case EventUserMessageType.Warning:
          Debug.LogWarning(noNewlinesMessage);
          break;
      }
    }

    public void Cancel()
    {
      if (!asyncState.Cancel)
      {
        Debug.Log("Cancelling...");
        asyncState.Cancel = true;
      }
    }

    private void ProcessEvent(AsyncEvent asyncEvent)
    {
      switch (asyncEvent.Type)
      {
        case AsyncEventType.Progress:
          display.ReportProgress(asyncEvent.PercentDone, asyncEvent.Message);
          break;
        case AsyncEventType.Info:
          Debug.Log(asyncEvent.Message);
          break;
        case AsyncEventType.Error:
          display.Clear();
          Debug.LogError(asyncEvent.Message);
          break;
        case AsyncEventType.UserMessage:
          DisplayUserMessage(asyncEvent);
          break;
      }
    }

    private void ProcessEvents()
    {
      AsyncEvent asyncEvent;
      while ((asyncEvent = asyncState.queue.PollEvent()) != null)
      {
        ProcessEvent(asyncEvent);
      }
    }
  }
}
