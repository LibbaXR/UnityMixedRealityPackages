// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.Linq;
using ml.zi;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed partial class TargetViewPresenter
    {
        public Action<PeripheralTargetMode> OnTargetModeChanged;
        private GenericMenu modeMenu;

        public void SelectTargetModeWithoutNotify(PeripheralTargetMode targetMode)
        {
            SetModeIcon(targetMode);
        }

        public void UpdateModeMenu(uint controllers)
        {
            modeMenu = new GenericMenu();
            modeMenu.AddItem(new GUIContent("Headpose"), false, () => SelectTargetMode(PeripheralTargetMode.Headpose));
            modeMenu.AddItem(new GUIContent("Scene View Camera"), false, () => SelectTargetMode(PeripheralTargetMode.SceneView));
            if (controllers > 0)
            {
                modeMenu.AddItem(new GUIContent("Control 1"), false, () => SelectTargetMode(PeripheralTargetMode.Control1));
            }

            if (controllers > 1)
            {
                modeMenu.AddItem(new GUIContent("Control 2"), false, () => throw new NotImplementedException());
            }

            modeMenu.AddItem(new GUIContent("Left Hand"), false, () => SelectTargetMode(PeripheralTargetMode.HandLeft));
            modeMenu.AddItem(new GUIContent("Right Hand"), false, () => SelectTargetMode(PeripheralTargetMode.HandRight));
            modeMenu.AddItem(new GUIContent("Eye Fixation"), false, () => SelectTargetMode(PeripheralTargetMode.EyeFixation));
        }

        public void SetErrorMessage(bool isError, string message = "")
        {
            errorLabel.SetDisplay(isError);
            errorLabel.text = message;
            targetConnectButton.SetEnabled(!isError);
        }

        private void ClearModeIcon()
        {
            string css = currentModeIcon.GetClasses().FirstOrDefault(x => x.StartsWith("targetMode-"));
            if (!string.IsNullOrEmpty(css))
            {
                currentModeIcon.RemoveFromClassList(css);
            }
        }

        private void InitModeMenu()
        {
            UpdateModeMenu();
            // OnSceneViewCameraSelected();
        }

        private void SelectTargetMode(PeripheralTargetMode targetMode)
        {
            SetModeIcon(targetMode);
            OnTargetModeChanged?.Invoke(targetMode);
        }

        private void SetModeIcon(PeripheralTargetMode targetMode)
        {
            ClearModeIcon();
            currentModeIcon.AddToClassList($"targetMode-{targetMode}");
        }

        private void UpdateModeMenu()
        {
            OnGetControllersQuery();
        }
    }
}
