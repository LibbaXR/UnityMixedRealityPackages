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
using System.Collections.Generic;
using System.Linq;
using ml.zi;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed partial class SceneRenderViewPresenter
    {
        public event Action AddModelButtonHandler;
        public event Action AddRoomButtonHandler;
        public event Action<int> AnchorModeSelected;
        public event Action<int> CameraModeSelected;
        public event Action ClearSceneButtonHandler;
        public event Action<int> ManipulationModeSelected;
        public event Action<int> ReferenceModeSelected;
        public event Action ResetCameraSelected;
        public event Action<string, bool> ToggleGizmoHandler;

        private static readonly string checkIconClassName = "checkIcon";
        private static readonly string menuItemCheckElementClassName = "anchoredRightCheckbox";
        private readonly Dictionary<SceneViewCameraMode, VisualElement> cameraModeItems = new();
        private SceneViewCameraMode previousCameraMode;
        public override PeripheralInputSource InputSource => PeripheralInputSource.SceneView;

        public void SelectAnchorModeWithoutNotify(AnchorMode anchorMode)
        {
            UpdateAnchorModeView(anchorMode);
        }

        public void SelectCameraModeWithoutNotify(SceneViewCameraMode mode)
        {
            if (previousCameraMode == SceneViewCameraMode.Unknown)
            {
                previousCameraMode = mode;
            }

            UpdateCameraModeView(mode);
        }

        public void SelectManipulationModeWithoutNotify(ManipulationMode manipulationMode)
        {
            UpdateManipulationModeView(manipulationMode);
        }

        public void SelectReferenceModeWithoutNotify(ReferenceMode referenceMode)
        {
            UpdateReferenceModeView(referenceMode);
        }

        public override void ToggleConnectionStatusPanelDisplay(bool enablePanel)
        {
            connectionStatusPanel.SetDisplay(enablePanel);
        }

        public override void SwitchConnectionStatusMessage(bool isDeviceMode)
        {
            connectionStatusLabel.text = isDeviceMode ?
                DeviceRenderViewPresenter.connectionStatusMessageDeviceMode : DeviceRenderViewPresenter.connectionStatusMessageNonDeviceMode;
        }

        public override void ToggleToolbarButtonsEnabled(bool enableButtons)
        {
            addRoomButton.SetEnabled(enableButtons);
            addModelButton.SetEnabled(enableButtons);
            clearSceneButton.SetEnabled(enableButtons);
            manipulationModeMenuButton.SetEnabled(enableButtons);
            anchorModeMenuButton.SetEnabled(enableButtons);
            referenceModeMenuButton.SetEnabled(enableButtons);
            cameraModeMenuButton.SetEnabled(enableButtons);
            gizmosButton.SetEnabled(enableButtons);
        }

        private void BindCameraModeItems()
        {
            cameraModeItems.Add(SceneViewCameraMode.TopDown, cameraModeTopDown);
            cameraModeItems.Add(SceneViewCameraMode.Flight, cameraModeFreeFly);
            cameraModeItems.Add(SceneViewCameraMode.Unknown, cameraModeReset);
        }

        private void CloseActiveMenu()
        {
            if (currentlyOpenMenu != null)
            {
                currentlyOpenMenu.visible = false;
                currentlyOpenMenu = null;

                SetPickingMode(PickingMode.Position);
            }
        }

        private void MarkMenuItemSelected(VisualElement menuItem)
        {
            VisualElement parent = menuItem.parent;
            IEnumerable<VisualElement> menuItems = parent.Children().Where(c => c is Button);
            foreach (VisualElement item in menuItems)
            {
                VisualElement checkElement = item.Children().FirstOrDefault(c => c.ClassListContains(menuItemCheckElementClassName));
                if (checkElement != null)
                {
                    if (item == menuItem)
                    {
                        checkElement.AddToClassList(checkIconClassName);
                    }
                    else
                    {
                        if (checkElement.ClassListContains(checkIconClassName))
                        {
                            checkElement.RemoveFromClassList(checkIconClassName);
                        }
                    }
                }

                if (item == menuItem)
                {
                    if (!item.ClassListContains("menuItemSelected"))
                    {
                        item.AddToClassList("menuItemSelected");
                    }
                }
                else if (item.ClassListContains("menuItemSelected"))
                {
                    item.RemoveFromClassList("menuItemSelected");
                }
            }
        }

        private void OnAddModelButtonClicked()
        {
            AddModelButtonHandler?.Invoke();
        }

        private void OnAddRoomButtonClicked()
        {
            AddRoomButtonHandler?.Invoke();
        }

        private void OnAnchorModeClicked(ClickEvent evt)
        {
            var target = evt.target as VisualElement;
            int index = anchorModeMenu.IndexOf(target);

            AnchorModeSelected?.Invoke(index);

            UpdateAnchorModeView((AnchorMode) index);
        }

        private void OnAnchorModeMenuClicked(ClickEvent evt)
        {
            evt.StopImmediatePropagation();
            ToggleMenuVisualElement(anchorModeMenu);
        }

        private void OnCameraModeClicked(ClickEvent evt)
        {
            var target = evt.target as VisualElement;
            int index = cameraModeMenu.IndexOf(target);
            if (index > 2)
            {
                ResetCameraSelected?.Invoke();
            }
            else
            {
                previousCameraMode = (SceneViewCameraMode) index;
                CameraModeSelected?.Invoke(index);

                UpdateCameraModeView((SceneViewCameraMode) index);
            }
        }

        private void OnCameraModeMenuClicked(ClickEvent evt)
        {
            if (((VisualElement) evt.target).ClassListContains("menuItemButton"))
            {
                if (previousCameraMode != SceneViewCameraMode.Unknown)
                {
                    MarkMenuItemSelected(cameraModeItems[previousCameraMode]);
                }
                else
                {
                    MarkMenuItemSelected(evt.target as VisualElement);
                }
            }

            evt.StopImmediatePropagation();
            ToggleMenuVisualElement(cameraModeMenu);
        }

        private void OnClearSceneButtonClicked()
        {
            ClearSceneButtonHandler?.Invoke();
        }

        private void OnDynamicGizmoToggled(string name, ChangeEvent<bool> evt)
        {
            var target = (Toggle) evt.target;
            bool value = target.value;
            ToggleGizmoHandler?.Invoke(name, value);
        }

        private void OnGizmosButtonClicked(ClickEvent evt)
        {
            evt.StopImmediatePropagation();
            if (!(evt.target is Toggle))
            {
                ToggleMenuVisualElement(gizmosMenu);
            }
        }

        private void OnGizmoToggled(ChangeEvent<bool> evt)
        {
            var toggle = evt.target as Toggle;
            int index = toggle.parent.IndexOf(toggle);
            bool enabled = evt.newValue;
            if (index == 0)
            {
                foreach (string gizmoName in gizmoElementsDictionary.Keys)
                {
                    gizmoElementsDictionary.TryGetValue(gizmoName, out Toggle gizmo);
                    gizmo.SetValueWithoutNotify(enabled);
                    ToggleGizmoHandler?.Invoke(gizmoName, enabled);
                }
            }
        }

        private void OnManipulationMenuClicked(ClickEvent evt)
        {
            evt.StopImmediatePropagation();
            ToggleMenuVisualElement(manipulationModeMenu);
        }

        private void OnManipulationModeClicked(ClickEvent evt)
        {
            var target = evt.target as VisualElement;
            int index = manipulationModeMenu.IndexOf(target);

            ManipulationModeSelected?.Invoke(index);

            UpdateManipulationModeView((ManipulationMode) index);
        }

        private void OnMouseLeaveMenuItem(MouseLeaveEvent evt)
        {
            evt.StopImmediatePropagation();
            var element = evt.target as VisualElement;
            element.RemoveFromClassList("menuItemHovered");
        }

        private void OnMouseOverMenuItem(MouseEnterEvent evt)
        {
            evt.StopImmediatePropagation();
            var element = evt.target as VisualElement;
            element.AddToClassList("menuItemHovered");
        }

        private void OnReferenceModeClicked(ClickEvent evt)
        {
            var target = evt.target as VisualElement;
            int index = referenceModeMenu.IndexOf(target);

            ReferenceModeSelected?.Invoke(index);

            UpdateReferenceModeView((ReferenceMode) index);
        }

        private void OnReferenceModeMenuClicked(ClickEvent evt)
        {
            evt.StopImmediatePropagation();
            ToggleMenuVisualElement(referenceModeMenu);
        }

        private void RootClicked(ClickEvent evt)
        {
            CloseActiveMenu();
        }

        private void ToggleMenuVisualElement(VisualElement menuElement)
        {
            if (!menuElement.ClassListContains("dropdownMenu"))
            {
                return;
            }

            menuElement.visible = !menuElement.visible;
            menuElement.SetEnabled(menuElement.visible);
            if (menuElement.visible)
            {
                CloseActiveMenu();
                currentlyOpenMenu = menuElement;
            }
            else if (currentlyOpenMenu == menuElement)
            {
                currentlyOpenMenu = null;
            }

            SetPickingMode(menuElement.visible ? PickingMode.Ignore : PickingMode.Position);
        }

        private void UpdateAnchorModeView(AnchorMode anchorMode)
        {
            VisualElement target = anchorModeMenu.ElementAt((int) anchorMode);
            MarkMenuItemSelected(target);
            SetAnchorModeMenuIcon(anchorMode);
        }

        private void UpdateCameraModeView(SceneViewCameraMode mode)
        {
            if (mode == SceneViewCameraMode.Unknown)
            {
                mode = previousCameraMode;
            }

            VisualElement target = cameraModeMenu.ElementAt((int) mode);
            MarkMenuItemSelected(target);
            SetCameraModeMenuIcon(mode);
        }

        private void UpdateManipulationModeView(ManipulationMode manipulationMode)
        {
            VisualElement target = manipulationModeMenu.ElementAt((int) manipulationMode);

            MarkMenuItemSelected(target);
            SetManipulationModeMenuIcon(manipulationMode);
        }

        private void UpdateReferenceModeView(ReferenceMode referenceMode)
        {
            VisualElement target = referenceModeMenu.ElementAt((int) referenceMode);
            MarkMenuItemSelected(target);
            SetReferenceModeMenuIcon(referenceMode);
        }
    }
}
