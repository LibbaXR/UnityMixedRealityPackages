// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
//
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;
using System;
using System.IO;
using UnityEditor;
using UnityEditor.XR.MagicLeap;
using Debug = UnityEngine.Debug;

namespace MagicLeap.ZI
{
    internal sealed class MagicLeapSdkPathProvider
    {
        ~MagicLeapSdkPathProvider()
        {
            MagicLeapEditorPreferencesProvider.OnSDKPathChanged -= OnSDKPathChanged;
        }

        internal event Action OnMagicLeapSDKPathChanged = null;

        public const string Key_DiscoveredInstallRoot = "ZI_Discovered_Install_Root";

        private readonly EnvironmentPathProvider environmentPathProvider;

        public MagicLeapSdkPathProvider(EnvironmentPathProvider environmentPathProvider)
        {
            this.environmentPathProvider = environmentPathProvider;

            MagicLeapEditorPreferencesProvider.OnSDKPathChanged += OnSDKPathChanged;
        }

        public string Path { get; private set; } = null;
        public bool IsValid
        {
            get => isValid;
            private set
            {
                if (value == isValid)
                    return;

                isValid = value;
                OnMagicLeapSDKPathChanged?.Invoke();
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

            if (forceUpdate)
            {
                SessionState.EraseString(Key_DiscoveredInstallRoot);
            }

            try
            {
                if (string.IsNullOrEmpty(path))
                {
                    Debug.LogError(
                        "Magic Leap C SDK not set. For Magic Leap App Simulator, configure Preferences > External Tools > Magic Leap C SDK to point to a Magic Leap C SDK.");
                }
                else if (Directory.Exists(path) == false)
                {
                    Debug.LogError("Magic Leap C SDK location does not exist. " +
                        "For Magic Leap App Simulator, configure Preferences > External Tools > Magic Leap C SDK to point to a Magic Leap C SDK.");
                }
                else
                {
                    ml.zi.Environment.SetSdkPath(path);

                    isValidPath = true;

                    // ZI detection depends on the Lumin SDK (if not explicitly configured)
                    environmentPathProvider.Update(MagicLeapSDKUtil.AppSimRuntimePath, true);
                }
            }
            catch (ResultIsErrorException e)
            {
                IsValid = false;
                Debug.LogError(e);
            }

            Path = path;
            IsValid = isValidPath;
        }

        private void OnSDKPathChanged(string path) => Update(path, true);
    }
}
