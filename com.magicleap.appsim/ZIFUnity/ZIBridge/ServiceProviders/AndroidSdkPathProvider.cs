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
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed class AndroidSdkPathProvider
    {
        public string Path { get; private set; } = null;
        public bool IsValid { get; private set; } = false;

        public void Update(string path, bool forceUpdate)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (!forceUpdate)
            {
                if (string.IsNullOrEmpty(path) && IsValid && !string.IsNullOrEmpty(Path))
                {
                    // auto-detected already
                    return;
                }
                if (path == Path)
                {
                    // no change
                    return;
                }
            }

            IsValid = false;

            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Environment.SetAndroidSdkPath(path);
                    IsValid = true;
                    if (ZIBridge.Debugging)
                    {
                        LogUtils.LogOncePerSession($"[ZIF] Using configured Android SDK in {path}", Settings.Key_LastAndroidSdkPathLogMessage);
                    }
                }
                else
                {
                    Environment.SetAndroidSdkPath(null);
                    path = Environment.GetAndroidSdkPath();
                    IsValid = true;
                    if (ZIBridge.Debugging)
                    {
                        LogUtils.LogOncePerSession($"[ZIF] Using detected Android SDK in {path}", Settings.Key_LastAndroidSdkPathLogMessage);
                    }
                }
            }
            catch (ResultIsErrorException e)
            {
                // ignore this -- we can still locate adb otherwise
                Debug.LogErrorFormat("[ZIF] Android SDK not found: {0}", ZIFGen.ResultGetString(e.Result));
            }

            try
            {
                // adb might be found via Android SDK or env vars
                Environment.SetAdbPath(null);
                IsValid = true;
                LogUtils.LogOncePerSession($"[ZIF] Using adb in {Environment.GetAdbPath()}", Settings.Key_LastAndroidSdkPathLogMessage);
            }
            catch (ResultIsErrorException e)
            {
                Debug.LogError("[ZIF] Failed to detect adb; App Sim Device mode will not work: " + e.Message + "\n" +
                    "(configure the Android SDK path, ANDROID_HOME or ANDROID_SDK_ROOT environment variable, or put 'adb' on PATH)");
            }

            Path = path;
        }
    }
}
