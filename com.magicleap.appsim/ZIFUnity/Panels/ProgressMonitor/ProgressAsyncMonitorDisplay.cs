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
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class ProgressAsyncMonitorDisplay : ProgressMonitorCallback
    {
        private readonly float timeout;
        private float elapsedTime;
        private int progressId;
        private float startTime;
        public bool Cancelled { get; private set; }
        public string Description { get; set; }
        public bool IsDone { get; private set; }
        public bool IsRunning { get; private set; }

        public bool Started { get; private set; }

        public string Title { get; set; }

        public ProgressAsyncMonitorDisplay(string description = "", string title = "Magic Leap App Simulator", float maxTime = 10f)
        {
            Started = false;
            IsDone = false;
            Cancelled = false;
            Title = title;
            Description = description;

            timeout = maxTime;
        }

        public static ProgressMonitor Show(string description = "")
        {
            return ProgressMonitor.Alloc(new ProgressAsyncMonitorDisplay(description));
        }

        public static ProgressMonitor Show(string description, string title)
        {
            return ProgressMonitor.Alloc(new ProgressAsyncMonitorDisplay(description, title));
        }

        public override ProgressMonitorCallbackResult Callback(ProgressMonitor progress_monitor, ProgressMonitorState state)
        {
            switch (state)
            {
                case ProgressMonitorState.Start:
                    Started = true;
                    IsDone = false;
                    Cancelled = false;
                    progressId = Progress.Start("ZI ProgressMonitor");
                    EditorApplication.update += HandleProgressBar;
                    Progress.RegisterCancelCallback(progressId, () =>
                    {
                        Title = "Cancelling";
                        Cancelled = true;
                        return true;
                    });
                    break;
                case ProgressMonitorState.Continue:
                    IsRunning = true;
                    Progress.Report(progressId, elapsedTime / timeout, Description);
                    break;
                case ProgressMonitorState.Finished:
                    IsRunning = false;
                    Started = false;
                    IsDone = true;
                    Progress.Remove(progressId);
                    break;
            }

            if (Cancelled)
            {
                IsRunning = false;
                Started = false;
                return ProgressMonitorCallbackResult.Cancel;
            }

            return ProgressMonitorCallbackResult.Continue;
        }

        private void HandleProgressBar()
        {
            if (!IsDone && !Cancelled)
            {
                if (Started && !IsRunning)
                {
                    startTime = Time.time;
                }
                else
                {
                    elapsedTime = Time.time - startTime;
                }

                if (EditorUtility.DisplayCancelableProgressBar(Title, Description, elapsedTime / timeout))
                {
                    Title = "Cancelling";
                    Cancelled = true;
                }
            }
            else
            {
                EditorApplication.update -= HandleProgressBar;
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
