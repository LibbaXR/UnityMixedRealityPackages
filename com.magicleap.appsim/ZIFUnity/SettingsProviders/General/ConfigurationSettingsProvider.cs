// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2019-2022) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Collections.Generic;
using System.IO;
using ml.zi;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using Environment = ml.zi.Environment;

namespace MagicLeap.ZI
{
    internal static class ConfigurationSettingsProvider
    {
        private static ConfigurationSettingsViewModel model;
        private static ConfigurationSettingsViewPresenter presenter;

        [SettingsProvider]
        public static UnityEditor.SettingsProvider CreateVirtualRoomSettingsProvider()
        {
            var provider = new UnityEditor.SettingsProvider("MagicLeap/App Simulator/ConfigurationSettings", SettingsScope.Project)
            {
                label = "General",
                activateHandler = (searchContext, root) =>
                {
                    var panel = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                        "Packages/com.magicleap.appsim/ZIFUnity/SettingsProviders/General/ConfigurationSettingsView.uxml");
                    panel.CloneTree(root);

                    presenter = new ConfigurationSettingsViewPresenter(root);
                    model = new ConfigurationSettingsViewModel();

                    SubscribeToEvents();
                    InitializeModel();
                    InitializeView();
                },
                keywords = new HashSet<string>(new[] { "ZI", "Zero", "Iteration", "Default", "Room", "Virtual" })
            };

            return provider;
        }

        private static void InitializeModel()
        {
            model.OnSetDefaultSessionPath += presenter.SetDefaultRoomPath;
            model.Initialize();
        }

        internal static void InitializeView()
        {
            presenter.SetLogLevel(model.GetLogLevel());
            presenter.SetHardwareAcceleration(model.GetHardwareAcceleration());
            presenter.SetDefaultRoomPath(model.GetDefaultRoomPath());
            presenter.SetDirtySessionState(model.GetDirtySessionState());
            presenter.SetCloseSessionDialog(model.GetCloseSessionDialog());
            presenter.SetCloseZiSessionOnQuit(model.GetCloseZiSessionOnQuit());
        }

        private static void SubscribeToEvents()
        {
            presenter.OnLogLevelChanged += model.SetLogLevel;
            presenter.OnHardwareAccelerationChanged += model.SetHardwareAcceleration;
            presenter.OnDefaultRoomPathChanged += model.SetDefaultSessionPath;
            presenter.OnDirtySessionChanged += model.SetDirtySession;
            presenter.OnShowCloseSessionDialogChanged += model.SetCloseSessionDialog;
            presenter.OnCloseZiSessionOnQuitChanged += model.SetCloseZiSessionOnQuit;
            presenter.OnResetButtonClicked += model.ResetSettingsToDefaults;
        }
    }

    internal class ConfigurationSettingsViewModel : ViewModel
    {
        public event Action<string> OnSetDefaultSessionPath;

        public string GetDefaultRoomPath()
        {
            return Settings.Instance.DefaultSessionPath;
        }

        public Settings.DirtySessionState GetDirtySessionState()
        {
            return Settings.Instance.DirtySessionPrompt;
        }

        public bool GetHardwareAcceleration()
        {
            return Settings.Instance.HardwareAcceleration;
        }

        public bool GetCloseSessionDialog()
        {
            return Settings.Instance.ShowCloseDialog;
        }

        public bool GetCloseZiSessionOnQuit()
        {
            return Settings.Instance.CloseZISessionOnQuit;
        }

        public LoggingLevel GetLogLevel()
        {
            return Settings.Instance.LoggingLevel;
        }

        public void SetDefaultSessionPath(string defaultSessionPath)
        {
            string installRoot = Environment.GetInstallRoot().Replace(@"\", "/");
            if (StringExtensions.IsSubPathOf(defaultSessionPath, installRoot))
            {
                defaultSessionPath = defaultSessionPath.Replace(installRoot, string.Empty);
            }
            Settings.Instance.DefaultSessionPath = defaultSessionPath;
            OnSetDefaultSessionPath?.Invoke(Settings.Instance.DefaultSessionPath);
        }

        public void SetDirtySession(Settings.DirtySessionState dirtySessionState)
        {
            Settings.Instance.DirtySessionPrompt = dirtySessionState;
        }

        public void SetHardwareAcceleration(bool hardwareAcceleration)
        {
            Settings.Instance.HardwareAcceleration = hardwareAcceleration;
        }

        public void SetCloseSessionDialog(bool closeSessionDialog)
        {
            Settings.Instance.ShowCloseDialog = closeSessionDialog;
        }

        public void SetCloseZiSessionOnQuit(bool closeSessionOnQuit)
        {
            Settings.Instance.CloseZISessionOnQuit = closeSessionOnQuit;
        }

        public void SetLogLevel(LoggingLevel loggingLevel)
        {
            Settings.Instance.LoggingLevel = loggingLevel;
        }


        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected;
        }

        public void ResetSettingsToDefaults()
        {
            Settings.Instance.SetDefaults();
            Bridge.Reinitialize();
            ConfigurationSettingsProvider.InitializeView();
        }

        private static class StringExtensions
        {
            /// <summary>
            /// Returns true if <paramref name="path"/> starts with the path <paramref name="baseDirPath"/>.
            /// The comparison is case-insensitive, handles / and \ slashes as folder separators and
            /// only matches if the base dir folder name is matched exactly ("c:\foobar\file.txt" is not a sub path of "c:\foo").
            /// </summary>
            public static bool IsSubPathOf(string path, string baseDirPath)
            {
                string normalizedPath = WithEnding(Path.GetFullPath(path.Replace('/', '\\')), "\\");
                string normalizedBaseDirPath = WithEnding(Path.GetFullPath(baseDirPath.Replace('/', '\\')), "\\");

                return normalizedPath.StartsWith(normalizedBaseDirPath, StringComparison.OrdinalIgnoreCase);
            }

            /// <summary>
            /// Returns <paramref name="str"/> with the minimal concatenation of <paramref name="ending"/> (starting from end) that
            /// results in satisfying .EndsWith(ending).
            /// </summary>
            /// <example>"hel".WithEnding("llo") returns "hello", which is the result of "hel" + "lo".</example>
            public static string WithEnding(string str, string ending)
            {
                if (str == null)
                    return ending;

                string result = str;

                // Right() is 1-indexed, so include these cases
                // * Append no characters
                // * Append up to N characters, where N is ending length
                for (int i = 0; i <= ending.Length; i++)
                {
                    string tmp = result + Right(ending, i);
                    if (tmp.EndsWith(ending))
                        return tmp;
                }

                return result;
            }

            /// <summary>Gets the rightmost <paramref name="length" /> characters from a string.</summary>
            /// <param name="value">The string to retrieve the substring from.</param>
            /// <param name="length">The number of characters to retrieve.</param>
            /// <returns>The substring.</returns>
            public static string Right(string value, int length)
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value");
                }
                if (length < 0)
                {
                    throw new ArgumentOutOfRangeException("length", length, "Length is less than zero");
                }

                return (length < value.Length) ? value.Substring(value.Length - length) : value;
            }
        }
    }
}
