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
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed class CompositeTaskDisplay : IAsyncTaskDisplay
    {
        private readonly List<IAsyncTaskDisplay> displays = new();

        public CompositeTaskDisplay(TaskDisplaySettings settings, string taskName, string taskDescription, Action onCancel = null)
        {
            if (settings.HasFlag(TaskDisplaySettings.ProgressWindow))
            {
                displays.Add(new WindowTaskDisplay(taskName, onCancel));
            }
            if (settings.HasFlag(TaskDisplaySettings.ProgressBackground))
            {
                displays.Add(new BackgroundTaskDisplay(taskName, taskDescription, onCancel));
            }
        }

        public void ReportProgress(float progress, string message)
        {
            foreach(var display in displays)
            {
                display.ReportProgress(progress, message);
            }
        }

        public void Start()
        {
            foreach (var display in displays)
            {
                display.Start();
            }
        }

        public void Clear()
        {
            foreach (var display in displays)
            {
                display.Clear();
            }
        }
    }
}
