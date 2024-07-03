// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2023 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Json;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.XR.MagicLeap;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class AppSimPackageVersionsDialog : EditorWindow
    {
        struct PackageVersion
        {
            public string name;
            public string displayName;
            public string version;
            public string expectedVersion;
        }

        struct TableColumnWidths
        {
            public int maxPkgNameLength;
            public int maxPkgVerLength;
            public int maxPkgExpVerLength;
        }

        List<PackageVersion> pkgVersions;
        GUIContent[,] tableContents;
        TableColumnWidths? tableColumnWidths;
        bool editorUpdating;   // true when the editor is processing updates (new assets, packages, etc)

        GUIContent testPassedIcon, testFailedIcon, warningIcon, clipboardIcon, invalidRuntimeHelpMsg;
        PackageVersion appSimForUnityVersion;
        GUIStyle oddRowStyle, evenRowStyle, helpTextLabelStyle;

        static string MSG_HELP_TEXT = "The <b>Current</b> column shows the version of installed Magic Leap packages that are in use. " +
            "The <b>Expected</b> column shows the package version that was built and tested against this version of App Sim.";
        static string MSG_UNKNOWN_VERSION = "Unknown";
        static string MSG_INVALID_VERSION = "Invalid*";
        static string MSG_STATUS_UNKNOWN = "Wasn't able to determine the version based on the install path. Running a local build?";
        static string MSG_STATUS_SUCCESS = "Package version matches the expected value.";
        static string MSG_STATUS_FAIL = "Package version does not match the expected value.";
        static string MSG_INVALID_RUNTIME_PATH = "*App Sim Runtime path is not detected. Please check the settings in <b>Preferences > External Tools > Magic Leap</b>.";

        static string dependencyFileName = "unity-appsim-dependencies.json";

        const int kMinDisplayNameFieldLength = 150;
        const int kMinVersionFieldLength = 120;

        const int kMaxDisplayNameFieldLength = 150;
        const int kMaxVersionFieldLength = 220;

        [MenuItem("Window/Magic Leap App Simulator/App Sim Diagnostic Tool")]
        static void ShowPackageVersionsWindow()
        {
            AppSimPackageVersionsDialog window = GetWindow<AppSimPackageVersionsDialog>(false, "App Sim Diagnostic Tool");
            window.minSize = new Vector2(380, 200);
            window.Show();
        }

        #region Collect Version Info
        private static UnityEditor.PackageManager.PackageInfo FindPackageInfoByName(string name)
        {
            return UnityEditor.PackageManager.PackageInfo.GetAllRegisteredPackages().Where(info => info.name == name).FirstOrDefault();
        }

        private static IEnumerable<UnityEditor.PackageManager.DependencyInfo> GetMagicLeapDependencies(UnityEditor.PackageManager.PackageInfo info)
        {
            return info.dependencies.Where(dep => dep.name.Contains("magicleap"));
        }

        private static void AddDependenciesToList(List<PackageVersion> pkgVersions, UnityEditor.PackageManager.PackageInfo info)
        {
            foreach (var dep in GetMagicLeapDependencies(info))
            {
                var depInfo = FindPackageInfoByName(dep.name);
                pkgVersions.Add(new PackageVersion() { name = depInfo.name, displayName = depInfo.displayName, version = depInfo.version, expectedVersion = dep.version });
                AddDependenciesToList(pkgVersions, depInfo);
            }
        }

        private static KeyValuePair<PackageVersion, List<PackageVersion>> CollectInfo()
        {
            var appSimInfo = UnityEditor.PackageManager.PackageInfo.FindForAssembly(System.Reflection.Assembly.GetExecutingAssembly());
            List<PackageVersion> pkgVersions = new List<PackageVersion>();
            PackageVersion appsimForUnityVer = new PackageVersion() { name = appSimInfo.name, displayName = appSimInfo.displayName, version = appSimInfo.version };

            AddDependenciesToList(pkgVersions, appSimInfo);

            string appSimVersion;
            if (ZIBridge.environmentPathProvider.IsValid == false && ZIBridge.BackendPath == ZIBridge.environmentPathProvider.Path)
            {
                appSimVersion = MSG_INVALID_VERSION;
            }
            else
            {
                var match = Regex.Match(ZIBridge.BackendPath, @".*[\/\\]v([0-9.]+[a-zA-Z_]*)$");
                appSimVersion = match.Success ? match.Groups[1].Value : null;
            }
            pkgVersions.Add(new PackageVersion() { name = "appsim.runtime", displayName = "App Sim Runtime", version = appSimVersion });

            // load dependency overrides for ExpectedVersion
            string absolute = Path.GetFullPath(Path.Combine(Constants.zifEditorPath, dependencyFileName));
            if (File.Exists(absolute))
            {
                try
                {
                    Dictionary<string, string> dict;  // key=pkg name, value=version
                    DataContractJsonSerializer ser = new DataContractJsonSerializer(typeof(Dictionary<string, string>),
                        new DataContractJsonSerializerSettings() { UseSimpleDictionaryFormat = true });
                    // read json file, removing single line comments (ie. comments starting with "//" )
                    using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(Regex.Replace(File.ReadAllText(absolute, Encoding.UTF8), @"//.*$", "", RegexOptions.Multiline))))
                    {
                        dict = ser.ReadObject(ms) as Dictionary<string, string>;
                    }
                    foreach (var kv in dict)
                    {
                        if (string.IsNullOrWhiteSpace(kv.Value))
                        {
                            Debug.LogError($"{Path.GetFileName(absolute)}: key={kv.Key}, value field is blank.");
                            continue;
                        }
                        int index;
                        if ((index = pkgVersions.FindIndex(pkgver => pkgver.name == kv.Key)) < 0)
                        {
                            Debug.LogWarning($@"{Path.GetFileName(absolute)}: key=""{kv.Key}"" is not an expected package name. Choices are: {string.Join(", ", pkgVersions.Select(p => @$"""{p.name}"""))}");
                            continue;
                        }
                        var val = pkgVersions[index];
                        val.expectedVersion = kv.Value;
                        pkgVersions[index] = val;
                    }
                }
                catch (System.Exception ex)
                {
                    Debug.LogError($"Failed to load contents of file({absolute}): {ex.Message}");
                }
            }
            else
            {
                Debug.LogError($"Dependency file not found: {absolute}");
            }
            // filter out entries where we have no expected value to compare against
            pkgVersions = pkgVersions.Where(pkgVer => !string.IsNullOrWhiteSpace(pkgVer.expectedVersion)).ToList();

            return KeyValuePair.Create(appsimForUnityVer, pkgVersions);
        }

        // Get a text version of the table suitable for pasting to the clipboard
        static string GetText(List<PackageVersion> pkgVersions, PackageVersion appSimForUnityVersion)
        {
            appSimForUnityVersion.expectedVersion = "  -"; // don't print "unknown" for this one
            List<PackageVersion> versions = new List<PackageVersion>(pkgVersions.Count + 1) { appSimForUnityVersion };
            versions.AddRange(pkgVersions);
            StringBuilder sb = new StringBuilder();
            int maxPkgNameLength = versions.Select(v => v.displayName.Length).Max();
            int maxPkgVerLength = versions.Select(v => (v.version ?? MSG_UNKNOWN_VERSION).Length).Max();
            int maxPkgExpVerLength = versions.Select(v => (v.expectedVersion ?? MSG_UNKNOWN_VERSION).Length).Max();

            sb.AppendLine(string.Format("{0}  {1}  {2}", "Package".PadRight(maxPkgNameLength), "Current".PadRight(maxPkgVerLength), "Expected".PadRight(maxPkgExpVerLength)));
            sb.Append('-', maxPkgNameLength+maxPkgVerLength+Mathf.Max(maxPkgExpVerLength, "Expected".Length)+4); // +4 for the spaces between columns
            sb.AppendLine();

            foreach (var pkgVersion in versions)
            {
                sb.Append(pkgVersion.displayName.PadRight(maxPkgNameLength));
                sb.Append(' ', 2);
                sb.Append((pkgVersion.version ?? MSG_UNKNOWN_VERSION).PadRight(maxPkgVerLength));
                sb.Append(' ', 2);
                sb.Append((pkgVersion.expectedVersion ?? MSG_UNKNOWN_VERSION).PadRight(maxPkgExpVerLength));
                sb.AppendLine();
            }

            return sb.ToString();
        }
        #endregion // Collect Version Info

        private void OnEnable()
        {
            testPassedIcon = EditorGUIUtility.IconContent("TestPassed");
            testFailedIcon = EditorGUIUtility.IconContent("Error");
            warningIcon = EditorGUIUtility.IconContent("Warning");
            clipboardIcon = EditorGUIUtility.IconContent("Clipboard");
            invalidRuntimeHelpMsg = new GUIContent(MSG_INVALID_RUNTIME_PATH, testFailedIcon.image);

            Color color = EditorGUIUtility.isProSkin ? new Color(0.30f, 0.30f, 0.30f) : new Color(0.80f, 0.80f, 0.80f);  // different background color for odd rows (visual clarity)
            Texture2D oddRowTexture = new Texture2D(1, 1);
            oddRowTexture.SetPixel(0, 0, color);
            oddRowTexture.Apply();
            oddRowStyle = new GUIStyle();
            oddRowStyle.normal.background = oddRowTexture;
            evenRowStyle = new GUIStyle();

            MagicLeapEditorPreferencesProvider.OnZIPathChanged += OnZiPathChanged;
            RefreshVersionInfo();
        }

        private void OnDisable()
        {
            MagicLeapEditorPreferencesProvider.OnZIPathChanged -= OnZiPathChanged;
        }

        private void OnZiPathChanged(string newPath)
        {
            editorUpdating = true;
            Repaint();
        }

        private void RefreshVersionInfo()
        {
            var kv = CollectInfo();
            appSimForUnityVersion = kv.Key;
            pkgVersions = kv.Value;

            tableContents = new GUIContent[pkgVersions.Count, 3];
            for (int i = 0; i < pkgVersions.Count; ++i)
            {
                var packageVersion = pkgVersions[i];

                GUIContent icon = warningIcon;
                string tooltip = MSG_STATUS_UNKNOWN;
                if (packageVersion.version != null && packageVersion.expectedVersion != null)
                {
                    icon = (packageVersion.version == packageVersion.expectedVersion) ? testPassedIcon : testFailedIcon;
                    tooltip = (packageVersion.version == packageVersion.expectedVersion) ? MSG_STATUS_SUCCESS : MSG_STATUS_FAIL;
                }

                tableContents[i, 0] = new GUIContent(packageVersion.displayName, icon.image, tooltip);
                tableContents[i, 1] = new GUIContent(packageVersion.version ?? MSG_UNKNOWN_VERSION);
                tableContents[i, 2] = new GUIContent(packageVersion.expectedVersion ?? MSG_UNKNOWN_VERSION);
            }

            tableColumnWidths = null;  // signal that columnsWidths must be recalculated -- must happen from inside the OnGUI call.
        }

        // Calculate the width of each column, based on the largest string in the column (measured in screen units).
        // Also applies tooltips to fields in the table where content would be cut off due to lack of space.
        private void CalcTableColumnWidths()
        {
            int maxPkgNameLength = kMinDisplayNameFieldLength;
            int maxPkgVerLength = kMinVersionFieldLength;
            int maxPkgExpVerLength = kMinVersionFieldLength;

            for (int i = 0; i < pkgVersions.Count; ++i)
            {
                var length = Mathf.CeilToInt(EditorStyles.label.CalcSize(tableContents[i, 0]).x);
                maxPkgNameLength = Mathf.Clamp(length, maxPkgNameLength, kMaxDisplayNameFieldLength);
                
                length = Mathf.CeilToInt(EditorStyles.label.CalcSize(tableContents[i, 1]).x);
                maxPkgVerLength = Mathf.Clamp(length, maxPkgVerLength, kMaxVersionFieldLength);
                if (length >= kMaxVersionFieldLength)
                {
                    tableContents[i, 1].tooltip = tableContents[i, 1].text;     // add tooltip to fields whose content will be truncated because it didn't fit
                }
                
                length = Mathf.CeilToInt(EditorStyles.label.CalcSize(tableContents[i, 2]).x);
                maxPkgExpVerLength = Mathf.Clamp(Mathf.CeilToInt(EditorStyles.label.CalcSize(tableContents[i, 2]).x), maxPkgExpVerLength, kMaxVersionFieldLength);
                if (length >= kMaxVersionFieldLength)
                {
                    tableContents[i, 2].tooltip = tableContents[i, 2].text;     // add tooltip to fields whose content will be truncated because it didn't fit
                }
            }

            tableColumnWidths = new TableColumnWidths() { maxPkgNameLength = maxPkgNameLength, maxPkgVerLength = maxPkgVerLength, maxPkgExpVerLength = maxPkgExpVerLength };
        }

        private void DrawFooter()
        {
            GUILayout.BeginVertical();
            {
                GUILayout.Space(5);

                GUILayout.BeginHorizontal();
                {
                    EditorGUILayout.LabelField(MSG_HELP_TEXT, helpTextLabelStyle ??= new GUIStyle(EditorStyles.label) { fontSize = 12, wordWrap = true, richText = true });
                    if (GUILayout.Button(new GUIContent(clipboardIcon.image, "Copy dialog contents to clipboard"), GUILayout.ExpandWidth(false)))
                    {
                        GUIUtility.systemCopyBuffer = GetText(pkgVersions, appSimForUnityVersion);
                    }
                }
                GUILayout.EndHorizontal();

                if (ZIBridge.environmentPathProvider.IsValid == false)
                {
                    GUILayout.Space(2);
                    EditorGUILayout.LabelField(invalidRuntimeHelpMsg, helpTextLabelStyle);
                }

                GUILayout.Space(2);
                EditorGUILayout.LabelField($"{appSimForUnityVersion.displayName}  v{appSimForUnityVersion.version}", EditorStyles.miniLabel);

            }
            GUILayout.EndVertical();
        }

        private void DrawTableHeader()
        {
            GUILayout.BeginHorizontal(EditorStyles.helpBox);

            GUILayout.Label("Package", EditorStyles.label, GUILayout.ExpandWidth(false), GUILayout.Width(tableColumnWidths.Value.maxPkgNameLength));
            GUILayout.Space(5);
            GUILayout.Label("Current", EditorStyles.label, GUILayout.ExpandWidth(false), GUILayout.Width(tableColumnWidths.Value.maxPkgVerLength));
            GUILayout.Space(5);
            GUILayout.Label("Expected", EditorStyles.label, GUILayout.ExpandWidth(false), GUILayout.Width(tableColumnWidths.Value.maxPkgExpVerLength));

            GUILayout.EndHorizontal();
            GUILayout.Space(2);
        }

        private void DrawPackageVersions()
        {
            for (int i = 0; i < pkgVersions.Count; ++i)
            {
                GUILayout.BeginHorizontal(i % 2 != 0 ? oddRowStyle : evenRowStyle);

                GUILayout.Label(tableContents[i,0], EditorStyles.label, GUILayout.ExpandWidth(false), GUILayout.Width(tableColumnWidths.Value.maxPkgNameLength));
                GUILayout.Space(10);
                GUILayout.Label(tableContents[i, 1], EditorStyles.label, GUILayout.ExpandWidth(false), GUILayout.Width(tableColumnWidths.Value.maxPkgVerLength));
                GUILayout.Space(5);
                GUILayout.Label(tableContents[i, 2], EditorStyles.label, GUILayout.ExpandWidth(false), GUILayout.Width(tableColumnWidths.Value.maxPkgExpVerLength));

                GUILayout.EndHorizontal();
                GUILayout.Space(2);
            }
        }

        private void RefreshVersionInfoAsync()
        {
            EditorCoroutineUtility.StartCoroutine(RefreshVersion(), this);

            IEnumerator RefreshVersion()
            {
                var waitForEndOfFrame = new WaitForEndOfFrame();
                yield return waitForEndOfFrame;
                RefreshVersionInfo();
                Repaint();
            }
        }

        private static bool EditorIsUpdating()
        {
            return AssetDatabase.IsAssetImportWorkerProcess() || EditorApplication.isUpdating || EditorApplication.isCompiling;
        }


        private void OnGUI()
        {
            if (editorUpdating != EditorIsUpdating() && (editorUpdating = !editorUpdating) == false)   // if we were updating, but not now
            {
                RefreshVersionInfoAsync(); // refresh version info outside of OnGUI
            }

            if (tableColumnWidths.HasValue == false)
            {
                CalcTableColumnWidths();
            }

            GUILayout.BeginVertical(EditorStyles.helpBox);
            {
                DrawTableHeader();
                DrawPackageVersions();
            }
            GUILayout.EndVertical();

            DrawFooter();
        }
    }
}
