// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using UnityEditor;

namespace MagicLeap.ZI
{
    internal sealed class BackgroundTaskDisplay : IAsyncTaskDisplay
    {
        private readonly string taskName;
        private readonly string taskDescription;
        private readonly Action onCancel;

        private int progressId;

        public BackgroundTaskDisplay(string taskName, string taskDescription, Action onCancel = null)
        {
            this.taskName = taskName;
            this.taskDescription = taskDescription;
            this.onCancel = onCancel;
        }

        public void ReportProgress(float progress, string message)
        {
            if (Progress.Exists(progressId))
            {
                Progress.Report(progressId, progress, message);
            }
        }

        public void Start()
        {
            if (string.IsNullOrEmpty(taskDescription))
            {
                progressId = Progress.Start(taskName);
            }
            else
            {
                progressId = Progress.Start(taskName, taskDescription);
            }

            Progress.RegisterCancelCallback(progressId, () =>
            {
                onCancel?.Invoke();
                return true;
            });
        }

        public void Clear()
        {
            if (Progress.Exists(progressId))
            {
                Progress.Finish(progressId);
            }
        }
    }
}
