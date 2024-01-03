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
using System.Collections.Generic;
using UnityEditor;

namespace MagicLeap.ZI
{
    internal class ProgressMonitorDisplay : ProgressMonitorCallback
    {
        protected readonly string description;
        protected readonly string title;
        protected readonly ulong maxTimeMs;

        protected int progressId;
        private ProgressMonitorState? previousState;

        protected readonly Queue<(ProgressMonitor, ProgressMonitorState)> callbacks = new();

        public ProgressMonitorDisplay(string description = "",
            string title = "Magic Leap App Simulator",
            ulong maxTimeMs = 1000)
        {
            this.title = title;
            this.maxTimeMs = maxTimeMs;
            this.description = description;

            EditorApplication.update += EditorUpdate;
        }

        public static ProgressMonitor Show(string description = "")
        {
            return ProgressMonitor.Alloc(new ProgressMonitorDisplay(description));
        }

        public static ProgressMonitor Show(string description, string title)
        {
            return ProgressMonitor.Alloc(new ProgressMonitorDisplay(description, title));
        }

        public override ProgressMonitorCallbackResult Callback(ProgressMonitor progressMonitor, ProgressMonitorState state)
        {
            callbacks.Enqueue((progressMonitor, state));

            return ProgressMonitorCallbackResult.Continue;
        }

        protected virtual void EditorUpdate()
        {
            while (callbacks.Count > 0)
            {
                var callbackArgs = callbacks.Dequeue();
                ProgressMonitor progressMonitor = callbackArgs.Item1;
                ProgressMonitorState monitorState = callbackArgs.Item2;

                switch ((previousState, monitorState))
                {
                    case (null, ProgressMonitorState.Continue):
                    case (null, ProgressMonitorState.Finished):
                    case (ProgressMonitorState.Start, ProgressMonitorState.Start):
                    case (ProgressMonitorState.Continue, ProgressMonitorState.Start):
                    case (ProgressMonitorState.Finished, ProgressMonitorState.Start):
                    case (ProgressMonitorState.Finished, ProgressMonitorState.Continue):
                    case (ProgressMonitorState.Finished, ProgressMonitorState.Finished):
                        continue;
                }

                previousState = monitorState;

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
                        break;
                }
            }
        }
    }
}
