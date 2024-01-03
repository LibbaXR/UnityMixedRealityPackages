// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class ProgressMonitorDisplayWithCallback : ProgressMonitorDisplay
    {
        private event Action onFinished;

        public ProgressMonitorDisplayWithCallback(string description = "",
            string title = "Magic Leap App Simulator",
            ulong maxTimeMs = 1000,
            Action onFinished = null) : base(description, title, maxTimeMs)
        { 
            this.onFinished += onFinished;
        }

        public static ProgressMonitor Show(string description = "", Action onFinished = null)
        {
            return ProgressMonitor.Alloc(new ProgressMonitorDisplayWithCallback(description, onFinished: onFinished));
        }

        public static ProgressMonitor Show(string description, string title, Action onFinished = null)
        {
            return ProgressMonitor.Alloc(new ProgressMonitorDisplayWithCallback(description, title, onFinished: onFinished));
        }

        protected override void EditorUpdate()
        {
            while (callbacks.Count > 0)
            {
                var callbackArgs = callbacks.Dequeue();
                ProgressMonitor progressMonitor = callbackArgs.Item1;
                ProgressMonitorState monitorState = callbackArgs.Item2;

                switch (monitorState)
                {
                    case ProgressMonitorState.Start:
                        progressId = Progress.Start(title, description);
                        break;
                    case ProgressMonitorState.Continue:
                        if (Progress.Exists(progressId))
                        {
                            Progress.Report(progressId, (float)progressMonitor.GetElapsedTimeMs() / maxTimeMs, description);
                        }
                        break;
                    case ProgressMonitorState.Finished:
                        if (Progress.Exists(progressId))
                        {
                            Progress.Finish(progressId);
                        }
                        EditorApplication.update -= EditorUpdate;
                        onFinished?.Invoke();
                        break;
                }
            }
        }
    }
}
