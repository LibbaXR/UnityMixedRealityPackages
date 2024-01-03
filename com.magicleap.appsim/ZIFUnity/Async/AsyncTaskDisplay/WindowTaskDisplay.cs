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
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed class WindowTaskDisplay : IAsyncTaskDisplay
    {
        private readonly string taskName;
        private readonly Action onCancel;

        public WindowTaskDisplay(string taskName, Action onCancel = null)
        {
            this.taskName = taskName;
            this.onCancel = onCancel;
        }

        public void ReportProgress(float progress, string message)
        {
            // NOTE: this preferred progress bar seems to have problems
            // in Windows Unity <2020.2, where the bar does not appear,
            // or blinks out of existence before it can be seen.
            if (EditorUtility.DisplayCancelableProgressBar(taskName, message, progress))
            {
                onCancel?.Invoke();
            }
        }

        public void Start() { }

        public void Clear()
        {
            EditorUtility.ClearProgressBar();
        }
    }
}
