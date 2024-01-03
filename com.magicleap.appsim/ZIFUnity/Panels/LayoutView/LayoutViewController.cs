// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.IO;
using MagicLeap.ZI;
using UnityEditor;
using UnityEditorInternal;

namespace ZIFUnity.Panels.LayoutView
{
    public static class LayoutViewController
    {
        private const string LayoutFileName = "AppSimulatorDefaultLayout.wlt";
        private static bool TipPresented = false;

        private static readonly string DefaultLayoutFile =
            $"Packages/com.magicleap.appsim/ZIFUnity/Panels/LayoutView/{LayoutFileName}";

        private static readonly string TargetLayoutFolder =
            Path.Combine(InternalEditorUtility.unityPreferencesFolder, "Layouts", "AppSimulator");

        [MenuItem("Window/Magic Leap App Simulator/Load Default Layout", false, PanelWindow.MenuItemPriority_LoadDefaultLayout)]
        public static void LoadDefaultLayout()
        {
            var targetFile = Path.Combine(TargetLayoutFolder, LayoutFileName);
            if (!Directory.Exists(TargetLayoutFolder)) Directory.CreateDirectory(TargetLayoutFolder);
            //Unity cant load layout files if its present in a package, so move
            //the layout file to where Unity stores layouts.
            File.Copy(DefaultLayoutFile, targetFile, true);
            EditorUtility.LoadWindowLayout(targetFile);
            if (!TipPresented)
            {
                TipPresented = true;
                EditorUtility.DisplayDialog("App Simulator",
                        "You can start and stop App Sim using the App Sim Target Panel", "OK");
                EditorWindow.GetWindow<TargetViewController>().Focus();
            }
        }
    }
}
