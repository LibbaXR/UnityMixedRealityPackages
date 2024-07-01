// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.IO;
using UnityEditor;
using UnityEngine;
using Environment = System.Environment;

namespace MagicLeap.ZI
{
    [InitializeOnLoad]
    internal class RuntimeSupport
    {
        static RuntimeSupport()
        {
            EditorApplication.playModeStateChanged += CheckIfEnteredPlayMode;
        }

        private static void CheckIfEnteredPlayMode(PlayModeStateChange playModeState)
        {
            if (playModeState == PlayModeStateChange.EnteredPlayMode)
            {
                OnEnteredPlayMode();
            }

            if (playModeState == PlayModeStateChange.ExitingPlayMode)
            {
                OnExitPlayMode();
            }
        }

        private static void OnEnteredPlayMode()
        {
            if (Settings.Instance.GameViewPoseDriverEnabled)
            {
                var harness = Camera.main.gameObject.AddComponent<ZIPoseHarness>();
                harness.SetDriver(new RuntimeHeadposeDriver());
            }
        }

        private static void OnExitPlayMode()
        {
        }
    }
}
