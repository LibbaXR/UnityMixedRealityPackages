// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEditor;
using UnityEditor.XR.MagicLeap;
using System.IO;

namespace MagicLeap.ZI
{
    internal sealed partial class ZIBridge
    {
        private static readonly AndroidSdkPathProvider androidSdkPathProvider = new AndroidSdkPathProvider();
        internal static readonly EnvironmentPathProvider environmentPathProvider = new EnvironmentPathProvider();
        internal static readonly MagicLeapSdkPathProvider luminSdkPathProvider = new MagicLeapSdkPathProvider(environmentPathProvider);

        public static string BackendPath
        {
            get
            {
                if (environmentPathProvider.IsValid == false)
                {
                    UpdateSDKPaths();
                }

                return environmentPathProvider.Path;
            }
        }

        public static string MLSDKPath
        {
            get => luminSdkPathProvider.Path;
        }

        public static string AndroidSDKPath
        {
            get => androidSdkPathProvider.Path;
        }

        public void Reinitialize()
        {
            UpdateSDKPaths(true);
        }

        public static void CheckVersionInfo()
        {
            environmentPathProvider.CheckVersionInfo();
        }

        private static void UpdateSDKPaths(bool forceUpdate = false)
        {
            luminSdkPathProvider.Update(GetSDKPathFromPreferencesWindow(), forceUpdate);
            androidSdkPathProvider.Update(GetAndroidSDKPathFromPreferencesWindow(), forceUpdate);
        }

        private static string GetSDKPathFromPreferencesWindow()
        {
            return MagicLeapSDKUtil.SdkPath;
        }

        private static string GetAndroidSDKPathFromPreferencesWindow()
        {
            string androidSDKPath = "";
            bool useEmbedded = EditorPrefs.GetBool("SdkUseEmbedded") || string.IsNullOrEmpty(EditorPrefs.GetString("AndroidSdkRoot"));
            if (useEmbedded)
            {
                androidSDKPath = Path.Combine(BuildPipeline.GetPlaybackEngineDirectory(BuildTarget.Android, BuildOptions.None), "SDK");
            }
            else
            {
                androidSDKPath = EditorPrefs.GetString("AndroidSdkRoot");
            }
            return androidSDKPath;
        }
    }
}
