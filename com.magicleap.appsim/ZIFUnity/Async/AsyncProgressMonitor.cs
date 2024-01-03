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
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class AsyncProgressMonitor<T> : ProgressMonitorCallback
    {
        private readonly string description;

        public AsyncTaskState<T> taskState;

        public AsyncProgressMonitor(string description)
        {
            this.description = description;
        }

        public override ProgressMonitorCallbackResult Callback(ProgressMonitor progressMonitor, ProgressMonitorState state)
        {
            switch (state)
            {
                case ProgressMonitorState.Start:
                    taskState.queue.QueueEvent(AsyncEventType.Progress, description, 0f);
                    break;
                case ProgressMonitorState.Continue:
                    var progress = Mathf.Clamp01((float)progressMonitor.GetElapsedTimeMs() / 1000);
                    taskState.queue.QueueEvent(AsyncEventType.Progress, description, progress);
                    break;
                case ProgressMonitorState.Finished:
                    taskState.Complete = true;
                    break;
            }

            return ProgressMonitorCallbackResult.Continue;
        }
    }
}
