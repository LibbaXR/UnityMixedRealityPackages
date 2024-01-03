// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using System.Linq;
using ml.zi;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed partial class SceneRenderViewPresenter : RenderViewPresenter<SceneRenderViewState>
    {
        private readonly Dictionary<string, Toggle> gizmoElementsDictionary = new();
        private Button addModelButton;

        private Button addRoomButton;
        private VisualElement anchorModeMenu;
        private Button anchorModeMenuButton;
        private VisualElement cameraModeFreeFly;
        private VisualElement cameraModeMenu;
        private Button cameraModeMenuButton;
        private VisualElement cameraModeReset;
        private VisualElement cameraModeTopDown;
        private Button clearSceneButton;
        private VisualElement connectionStatusPanel;

        private VisualElement currentlyOpenMenu;
        private Button gizmosButton;
        private VisualElement gizmosMenu;
        private VisualElement manipulationModeMenu;
        private Button manipulationModeMenuButton;
        private VisualElement referenceModeMenu;
        private Button referenceModeMenuButton;

        private new SceneRenderViewState State
        {
            get => base.State;
            set => base.State = value;
        }

        public void AddGizmo(SceneGizmo gizmo)
        {
            if (!gizmoElementsDictionary.TryGetValue(gizmo.Name, out _))
            {
                var newGizmo = new Toggle(gizmo.Label);
                newGizmo.name = gizmo.Name;
                newGizmo.tooltip = gizmo.Description;
                newGizmo.SetValueWithoutNotify(gizmo.Value);
                gizmoElementsDictionary.Add(gizmo.Name, newGizmo);
                if (!newGizmo.ClassListContains("gizmoToggle"))
                {
                    newGizmo.AddToClassList("gizmoToggle");
                }

                gizmosMenu.Add(newGizmo);
                newGizmo.RegisterValueChangedCallback(evt => OnDynamicGizmoToggled(gizmo.Name, evt));
            }
        }

        public void ClearGizmos()
        {
            foreach (Toggle gizmo in gizmoElementsDictionary.Values)
            {
                gizmosMenu.Remove(gizmo);
            }

            gizmoElementsDictionary.Clear();
        }

        public override void OnEnable(VisualElement root)
        {
            var visualTree =
                AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                    "Packages/com.magicleap.appsim/ZIFUnity/Panels/RenderWindows/Views/ZISceneRenderView.uxml");

            visualTree.CloneTree(root);

            base.OnEnable(root);
            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/RenderWindows/Views/ZIRenderWindowDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/RenderWindows/Views/ZIRenderWindowLightStyle.uss");

            manipulationModeMenu.visible = false;
            anchorModeMenu.visible = false;
            referenceModeMenu.visible = false;
            cameraModeMenu.visible = false;
            gizmosMenu.visible = false;
        }

        public void RemoveGizmo(string key)
        {
            Toggle disposableGizmo;
            gizmoElementsDictionary.TryGetValue(key, out disposableGizmo);
            gizmoElementsDictionary.Remove(key);
            gizmosMenu.Remove(disposableGizmo);
        }

        public void SetGizmoToggleValue(string key, bool enabled)
        {
            gizmoElementsDictionary.TryGetValue(key, out Toggle gizmo);
            if (gizmo != null)
            {
                gizmo.SetValueWithoutNotify(enabled);
            }
            else
            {
                Debug.LogErrorFormat("The specified gizmo key {0} does not exist.", key);
            }
        }

        protected override void AssertFields()
        {
            base.AssertFields();

            Assert.IsNotNull(addRoomButton, nameof(addRoomButton));
            Assert.IsNotNull(addModelButton, nameof(addModelButton));
            Assert.IsNotNull(clearSceneButton, nameof(clearSceneButton));
            Assert.IsNotNull(gizmosButton, nameof(gizmosButton));
            Assert.IsNotNull(manipulationModeMenuButton, nameof(manipulationModeMenuButton));
            Assert.IsNotNull(anchorModeMenuButton, nameof(anchorModeMenuButton));
            Assert.IsNotNull(referenceModeMenuButton, nameof(referenceModeMenuButton));
            Assert.IsNotNull(cameraModeMenuButton, nameof(cameraModeMenuButton));
            Assert.IsNotNull(manipulationModeMenu, nameof(manipulationModeMenu));
            Assert.IsNotNull(anchorModeMenu, nameof(anchorModeMenu));
            Assert.IsNotNull(referenceModeMenu, nameof(referenceModeMenu));
            Assert.IsNotNull(cameraModeMenu, nameof(cameraModeMenu));
            Assert.IsNotNull(gizmosMenu, nameof(gizmosMenu));
            Assert.IsNotNull(connectionStatusPanel, nameof(connectionStatusPanel));
        }

        protected override void BindUIElements()
        {
            base.BindUIElements();

            addRoomButton = Root.Q<Button>("AddRoomBtn");
            addModelButton = Root.Q<Button>("AddModelBtn");
            clearSceneButton = Root.Q<Button>("ClearSceneBtn");
            gizmosButton = Root.Q<Button>("GizmosBtn");

            manipulationModeMenuButton = Root.Q<Button>("ManipulationModeBtn");
            anchorModeMenuButton = Root.Q<Button>("AnchorModeBtn");
            referenceModeMenuButton = Root.Q<Button>("ReferenceModeBtn");
            cameraModeMenuButton = Root.Q<Button>("CameraModeBtn");

            manipulationModeMenu = Root.Q<VisualElement>("ManipulationModeMenu");
            anchorModeMenu = Root.Q<VisualElement>("AnchorModeMenu");
            referenceModeMenu = Root.Q<VisualElement>("ReferenceModeMenu");
            cameraModeMenu = Root.Q<VisualElement>("CameraModeMenu");
            cameraModeTopDown = Root.Q<VisualElement>("CameraMode-TopDown");
            cameraModeFreeFly = Root.Q<VisualElement>("CameraMode-FreeFly");
            cameraModeReset = Root.Q<VisualElement>("CameraMode-Reset");

            gizmosMenu = Root.Q<VisualElement>("GizmosMenu");
            connectionStatusPanel = Root.Q<VisualElement>("ConnectionStatusPanel");

            BindCameraModeItems();
        }

        protected override void DeSerialize()
        {
            if (EditorPrefs.HasKey(PlayerPrefsKey))
            {
                string savedJson = EditorPrefs.GetString(PlayerPrefsKey);
                State = JsonUtility.FromJson<SceneRenderViewState>(savedJson);
            }

            if (State != null)
            {
                return;
            }

            State = new SceneRenderViewState();
            State.SetDefaultValues();
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();

            addRoomButton.clicked += OnAddRoomButtonClicked;
            addModelButton.clicked += OnAddModelButtonClicked;
            clearSceneButton.clicked += OnClearSceneButtonClicked;
            gizmosButton.RegisterCallback<ClickEvent>(OnGizmosButtonClicked);
            manipulationModeMenuButton.RegisterCallback<ClickEvent>(OnManipulationMenuClicked);
            anchorModeMenuButton.RegisterCallback<ClickEvent>(OnAnchorModeMenuClicked);
            referenceModeMenuButton.RegisterCallback<ClickEvent>(OnReferenceModeMenuClicked);
            cameraModeMenuButton.RegisterCallback<ClickEvent>(OnCameraModeMenuClicked);

            foreach (VisualElement element in manipulationModeMenu.Children().Where(e => e is Button))
            {
                element.RegisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.RegisterCallback<ClickEvent>(OnManipulationModeClicked);
            }

            foreach (VisualElement element in anchorModeMenu.Children().Where(e => e is Button))
            {
                element.RegisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.RegisterCallback<ClickEvent>(OnAnchorModeClicked);
            }

            foreach (VisualElement element in referenceModeMenu.Children().Where(e => e is Button))
            {
                element.RegisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.RegisterCallback<ClickEvent>(OnReferenceModeClicked);
            }

            foreach (VisualElement element in cameraModeMenu.Children().Where(e => e is Button))
            {
                element.RegisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.RegisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.RegisterCallback<ClickEvent>(OnCameraModeClicked);
            }

            foreach (VisualElement element in gizmosMenu.Children().Where(e => e is Toggle))
            {
                ((Toggle) element).RegisterValueChangedCallback(OnGizmoToggled);
            }

            Root.RegisterCallback<ClickEvent>(RootClicked);
        }

        protected override void Serialize()
        {
            string json = JsonUtility.ToJson(State);
            EditorPrefs.SetString(PlayerPrefsKey, json);
        }

        protected override void UnregisterUICallbacks()
        {
            base.UnregisterUICallbacks();

            addRoomButton.clicked -= OnAddRoomButtonClicked;
            addModelButton.clicked -= OnAddModelButtonClicked;
            clearSceneButton.clicked -= OnClearSceneButtonClicked;
            gizmosButton.UnregisterCallback<ClickEvent>(OnGizmosButtonClicked);
            manipulationModeMenuButton.UnregisterCallback<ClickEvent>(OnManipulationMenuClicked);
            anchorModeMenuButton.UnregisterCallback<ClickEvent>(OnAnchorModeMenuClicked);
            referenceModeMenuButton.UnregisterCallback<ClickEvent>(OnReferenceModeMenuClicked);
            cameraModeMenuButton.UnregisterCallback<ClickEvent>(OnCameraModeMenuClicked);

            foreach (VisualElement element in manipulationModeMenu.Children().Where(e => e is Button))
            {
                element.UnregisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.UnregisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.UnregisterCallback<ClickEvent>(OnManipulationModeClicked);
            }

            foreach (VisualElement element in anchorModeMenu.Children().Where(e => e is Button))
            {
                element.UnregisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.UnregisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.UnregisterCallback<ClickEvent>(OnAnchorModeClicked);
            }

            foreach (VisualElement element in referenceModeMenu.Children().Where(e => e is Button))
            {
                element.UnregisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.UnregisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.UnregisterCallback<ClickEvent>(OnReferenceModeClicked);
            }

            foreach (VisualElement element in cameraModeMenu.Children().Where(e => e is Button))
            {
                element.UnregisterCallback<MouseEnterEvent>(OnMouseOverMenuItem);
                element.UnregisterCallback<MouseLeaveEvent>(OnMouseLeaveMenuItem);
                element.UnregisterCallback<ClickEvent>(OnCameraModeClicked);
            }

            foreach (VisualElement element in gizmosMenu.Children().Where(e => e is Toggle))
            {
                ((Toggle) element).UnregisterValueChangedCallback(OnGizmoToggled);
            }

            Root.UnregisterCallback<ClickEvent>(RootClicked);
        }

        private void ClearModeIcon(VisualElement element)
        {
            List<string> classes = element.GetClasses().ToList();
            for (int i = 0; i < classes.Count(); i++)
            {
                if (classes[i].EndsWith("Icon"))
                {
                    element.RemoveFromClassList(classes[i]);
                }
            }
        }

        private void SetAnchorModeMenuIcon(AnchorMode mode)
        {
            ClearModeIcon(anchorModeMenuButton);
            switch (mode)
            {
                case AnchorMode.Pivot:
                    anchorModeMenuButton.AddToClassList("anchorPivotModeIcon");
                    break;
                case AnchorMode.Center:
                    anchorModeMenuButton.AddToClassList("anchorCenterModeIcon");
                    break;
            }
        }

        private void SetCameraModeMenuIcon(SceneViewCameraMode mode)
        {
            ClearModeIcon(cameraModeMenuButton);
            switch (mode)
            {
                case SceneViewCameraMode.Flight:
                    cameraModeMenuButton.AddToClassList("freeFlyCameraModeIcon");
                    break;
                case SceneViewCameraMode.TopDown:
                    cameraModeMenuButton.AddToClassList("topDownCameraModeIcon");
                    break;
            }
        }

        private void SetManipulationModeMenuIcon(ManipulationMode mode)
        {
            ClearModeIcon(manipulationModeMenuButton);
            switch (mode)
            {
                case ManipulationMode.None:
                    manipulationModeMenuButton.AddToClassList("noneManipulationModeIcon");
                    break;
                case ManipulationMode.Move:
                    manipulationModeMenuButton.AddToClassList("moveManipulationModeIcon");
                    break;
                case ManipulationMode.Rotate:
                    manipulationModeMenuButton.AddToClassList("rotateManipulateModeIcon");
                    break;
                case ManipulationMode.Scale:
                    manipulationModeMenuButton.AddToClassList("scaleManipulateModeIcon");
                    break;
            }
        }

        private void SetReferenceModeMenuIcon(ReferenceMode mode)
        {
            ClearModeIcon(referenceModeMenuButton);
            switch (mode)
            {
                case ReferenceMode.Local:
                    referenceModeMenuButton.AddToClassList("localReferenceModeIcon");
                    break;
                case ReferenceMode.World:
                    referenceModeMenuButton.AddToClassList("worldReferenceModeIcon");
                    break;
            }
        }
    }
}
