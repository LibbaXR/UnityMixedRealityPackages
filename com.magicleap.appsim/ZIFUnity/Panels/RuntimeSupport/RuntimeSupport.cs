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
            // On Windows, adding the plugins directory to PATH variable will allow
            // the ML support libraries (e.g. perception.magicleap.dll) to successfully
            // locate their ZIF dependencies (e.g. z.dll).
            // This modified PATH variable exists only for the running Unity process
            if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                string currPathValue = Environment.GetEnvironmentVariable("PATH").TrimEnd(';');
                string absPkgPath = Path.GetFullPath(Constants.zifPackagePath);
                string pluginsPathToAdd = Path.Combine(absPkgPath, "Plugins", Application.platform.ToString());
                if (!currPathValue.Contains(pluginsPathToAdd))
                {
                    Environment.SetEnvironmentVariable("PATH", $"{currPathValue};{pluginsPathToAdd}");
                }
            }

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
            ZIBridge.Instance.OnSessionConnectedChanged += SessionConnectionStatusChanged;
            if (ZIBridge.Instance.IsConnected)
            {
                if (Settings.Instance.GameViewPoseDriverEnabled)
                {
                    var harness = Camera.main.gameObject.AddComponent<ZIPoseHarness>();
                    harness.SetDriver(new RuntimeHeadposeDriver());
                }
            }
        }

        private static void OnExitPlayMode()
        {
            ZIBridge.Instance.OnSessionConnectedChanged -= SessionConnectionStatusChanged;
        }

        private static void SessionConnectionStatusChanged(bool connectionStatus)
        {
            if (ZIBridge.IsSessionConnected)
                return;

            if (EditorApplication.isPlaying) 
                EditorApplication.isPlaying = false;
        }
    }
}
