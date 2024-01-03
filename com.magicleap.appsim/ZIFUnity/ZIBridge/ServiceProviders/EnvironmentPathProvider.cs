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
using System.Diagnostics;
using UnityEditor.XR.MagicLeap;
using Debug = UnityEngine.Debug;
using Environment = ml.zi.Environment;
using Version = ml.zi.Version;

namespace MagicLeap.ZI
{
    internal sealed class EnvironmentPathProvider
    {
        internal EnvironmentPathProvider()
        {
            MagicLeapEditorPreferencesProvider.OnZIPathChanged += OnZIPathChanged;
        }

        ~EnvironmentPathProvider()
        {
            MagicLeapEditorPreferencesProvider.OnZIPathChanged -= OnZIPathChanged;
        }

        internal event Action OnEnvironmentPathChanged = null;

        public string Path { get; private set; } = null;
        public bool IsValid
        {
            get => isValid;
            private set
            {
                if (value == isValid)
                    return;

                isValid = value;
                OnEnvironmentPathChanged?.Invoke();
            }
        }

        private bool isValid = false;

        public void Update(string path, bool forceUpdate)
        {
            if (path == null)
            {
                path = string.Empty;
            }

            if (path == Path && !forceUpdate)
            {
                return;
            }

            bool isValidPath = false;

            try
            {
                if (!string.IsNullOrEmpty(path))
                {
                    Environment.SetInstallRoot(path);
                    isValidPath = true;
                    LogUtils.LogOncePerSession($"[ZIF] App Sim runtime path discovered: {path}", Settings.Key_LastVersionLogMessage);

                    Path = path;
                    CheckVersionInfo();
                }
                else if (!MagicLeapSDKUtil.SearchingForZI)
                {
                    Debug.LogErrorFormat("[ZIF] App Sim runtime not found");
                }
            }
            catch (ResultIsErrorException e)
            {
                IsValid = false;
                Debug.LogErrorFormat("[ZIF] App Sim runtime not found: {0}", ZIFGen.ResultGetString(e.Result));
            }

            Path = path;
            IsValid = isValidPath;
        }

        public void CheckVersionInfo()
        {
            string bundledZIFVersion = Version.GetString();
            string runtimeZIFVersion = DetectRuntimeZIFVersion();

            if (bundledZIFVersion != runtimeZIFVersion)
            {
                // Technically only the non-datestamp versions are relevant.
                string relevantBundledVersion = FetchRelevantVersionInfo(bundledZIFVersion);
                string relevantRuntimeVersion = FetchRelevantVersionInfo(runtimeZIFVersion);
                if (relevantBundledVersion != relevantRuntimeVersion)
                {
                    Debug.LogErrorFormat(
                        "[ZIF] The ZIF version in the '{0}' package ({1}) differs from the one in the App Sim runtime ({2}).\n" +
                        "This will lead to problems.",
                        Constants.zifPackageName, relevantBundledVersion, relevantRuntimeVersion);
                }
            }

            LogUtils.LogOncePerSession($"[ZIF] ZIF version: {bundledZIFVersion} (bundled)  {runtimeZIFVersion} (runtime)", Settings.Key_LastEnvironmentPathLogMessage);
        }

        public string DetectRuntimeZIFVersion()
        {
            try
            {
                string installPath = Path;

                var startInfo = new ProcessStartInfo
                {
                    FileName = string.Format(@"{0}/bin/ZIDiscovery", installPath),
                    Arguments = "-v",
                    RedirectStandardInput = true,
                    UseShellExecute = false,
                    WindowStyle = ProcessWindowStyle.Hidden,
                    CreateNoWindow = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true
                };

#if UNITY_EDITOR_OSX
                // ignore any dev override
                startInfo.Environment.Remove("DYLD_LIBRARY_PATH");
#endif

                if (ZIBridge.Debugging)
                {
                    Debug.LogFormat("[ZIF] {0} {1}", startInfo.FileName, startInfo.Arguments);
                }

                var process = new Process
                {
                    EnableRaisingEvents = true,
                    StartInfo = startInfo
                };

                process.Start();

                string versionStr = "";
                string errorStr = "";
                do
                {
                    versionStr += process.StandardOutput.ReadToEnd();
                    errorStr += process.StandardError.ReadToEnd();
                } while (!process.HasExited);

                versionStr = versionStr.Trim();

                bool dumpInfo = false;
                string version = "";

                // format: "ZIF library: Version 0.0.15.20211202"
                int index = versionStr.IndexOf("Version ");
                if (index >= 0)
                {
                    version = versionStr.Substring(index);
                }
                else
                {
                    dumpInfo = true;
                    Debug.LogWarning("[ZIF] Did not detect runtime ZIF version!");
                }

                if (dumpInfo)
                {
                    Debug.LogFormat("[ZIF] ZIDiscovery -v returned: {0}{1}", versionStr, errorStr);
                }

                return version;
            }
            catch (Exception e)
            {
                Debug.LogError(e);
                return "";
            }
        }

        private static string FetchRelevantVersionInfo(string fullVersionStr)
        {
            fullVersionStr = StripVersionPrefix(fullVersionStr);
            int lastDot = fullVersionStr.LastIndexOf('.');
            if (lastDot >= 0)
            {
                return fullVersionStr.Substring(0, lastDot).Trim();
            }

            // empty or garbage
            return fullVersionStr;
        }

        private static string StripVersionPrefix(string versionStr)
        {
            const string versionMarker = "Version ";
            int index = versionStr.IndexOf(versionMarker);
            if (index >= 0)
            {
                return versionStr.Substring(index + versionMarker.Length).Trim();
            }

            return versionStr;
        }

        private void OnZIPathChanged(string path) => Update(path, false);
    }
}
