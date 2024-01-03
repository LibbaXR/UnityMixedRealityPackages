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
using System.Collections;
using System.Collections.Generic;
using NUnit.Framework;
using Unity.EditorCoroutines.Editor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class SceneGraphViewPresenter : ViewPresenter<SceneGraphViewState>
    {
        public event Action AddModelClicked = null;
        public event Action AddRoomClicked = null;
        public event Action ClearSceneClicked = null;

        public event Action DeleteButtonClicked = null;
        public event Action<string> NodeNameChanged
        {
            add
            {
                sceneObjectsGraph.NodeNameChanged += value;
                trackedObjectsGraph.NodeNameChanged += value;
                propertiesPresenter.OnNameFieldChanged += value;
            }
            remove
            {
                sceneObjectsGraph.NodeNameChanged -= value;
                trackedObjectsGraph.NodeNameChanged -= value;
                propertiesPresenter.OnNameFieldChanged -= value;
            }
        }
        
        public event Action<string> DeleteNodeClicked
        {
            add => sceneObjectsGraph.NodeDeleted += value;
            remove => sceneObjectsGraph.NodeDeleted -= value;
        }

        public event Action<CombinedVector3> NodeOrientationChanged
        {
            add => propertiesPresenter.OnOrientationFieldChanged += value;
            remove => propertiesPresenter.OnOrientationFieldChanged -= value;
        }

        public event Action<string> NodeParentChanged = delegate { };

        public event Action<CombinedVector3> NodePositionChanged
        {
            add => propertiesPresenter.OnPositionFieldChanged += value;
            remove => propertiesPresenter.OnPositionFieldChanged -= value;
        }

        public event Action<CombinedVector3> NodeScaleChanged
        {
            add => propertiesPresenter.OnScaleFieldChanged += value;
            remove => propertiesPresenter.OnScaleFieldChanged -= value;
        }

        public event Action<IReadOnlyList<string>> NodeSelected = delegate { };

        public event Action<LightPropertiesData> OnLightDataChanged
        {
            add => propertiesPresenter.LightProperties.OnLightDataChanged += value;
            remove => propertiesPresenter.LightProperties.OnLightDataChanged -= value;
        }

        private Button addModelButton;

        private Button addRoomButton;
        private VisualElement buttons;
        private Button clearSceneButton;

        private PropertiesPresenter propertiesPresenter;

        private VisualElement propertiesWindow;

        private ScrollView sceneHierarchyScrollView;
        private ToolbarToggle sceneHierarchyTab;

        private HierarchyPresenter sceneObjectsGraph;

        private VisualElement sceneObjectsView;
        private TwoPaneSplitView splitView;
        private VisualElement tabsContent;

        private Toolbar toolbar;
        private HierarchyPresenter trackedObjectsGraph;
        private ToolbarToggle trackedObjectsTab;

        private VisualElement trackedObjectsView;
        public IReadOnlyList<string> SelectedNodesID => State.SelectedNodesId;

        private HierarchyPresenter ActiveHierarchyPresenter => State.IsSceneObjectsTabOpened ? sceneObjectsGraph : trackedObjectsGraph;

        public void ClearListOfActiveFoldouts()
        {
            State.OpenedSceneObjectFoldoutsIds.Clear();
        }

        public void ClearSceneGraph()
        {
            sceneObjectsGraph.Clear();
        }

        public void ClearTrackedObjectsGraph()
        {
            trackedObjectsGraph.Clear();
        }

        public override void OnEnable(VisualElement root)
        {
            var visualTree = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/SceneGraph/Views/ZISceneGraphView.uxml");

            visualTree.CloneTree(root);

            base.OnEnable(root);
            splitView.fixedPaneIndex = 1;
            AddThemeStyleSheetToRoot(
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/SceneGraph/Views/ZISceneGraphDarkStyle.uss",
                "Packages/com.magicleap.appsim/ZIFUnity/Panels/SceneGraph/Views/ZISceneGraphLightStyle.uss");
        }

        public void SetEnabled(bool isModelInitialized)
        {
            buttons.SetEnabled(isModelInitialized);
            toolbar.SetEnabled(isModelInitialized);

            tabsContent.SetEnabled(isModelInitialized);
            propertiesWindow.SetEnabled(isModelInitialized);
        }

        public void SetNodeName(string selectedNodeId, string newName)
        {
            sceneObjectsGraph.SetNodeName(selectedNodeId, newName);
            trackedObjectsGraph.SetNodeName(selectedNodeId, newName);
        }

        public void SetPropertiesData(PropertiesViewData propertiesViewData)
        {
            propertiesPresenter.SetPropertiesData(propertiesViewData);
        }

        public void SetSceneObjectsGraph(IReadOnlyList<SceneGraphNodeData> sceneObjects)
        {
            if (sceneObjects == null || sceneObjectsGraph == null)
            {
                return;
            }

            sceneObjectsGraph.SetData(sceneObjects);
            sceneObjectsGraph.SetSelectedNodes(SelectedNodesID);
        }

        public void SetSelectedNodes(IReadOnlyList<string> selectedNodesId)
        {
            State.SelectedNodesId = selectedNodesId;
            sceneObjectsGraph.SetSelectedNodes(SelectedNodesID);
            trackedObjectsGraph.SetSelectedNodes(SelectedNodesID);

            if (SelectedNodesID.Count == 0)
            {
                return;
            }

            if (ActiveHierarchyPresenter.SelectedNode != null)
            {
                sceneHierarchyScrollView.ScrollTo(ActiveHierarchyPresenter.SelectedNode);
            }
        }

        public void SetTrackedObjectsGraph(IReadOnlyList<SceneGraphNodeData> trackedObjects)
        {
            if (trackedObjects == null || trackedObjectsGraph == null)
            {
                return;
            }

            trackedObjectsGraph.SetData(trackedObjects);
            trackedObjectsGraph.SetSelectedNodes(SelectedNodesID);
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(sceneHierarchyScrollView, nameof(sceneHierarchyScrollView));

            Assert.IsNotNull(addRoomButton, nameof(addRoomButton));
            Assert.IsNotNull(addModelButton, nameof(addModelButton));
            Assert.IsNotNull(clearSceneButton, nameof(clearSceneButton));

            Assert.IsNotNull(tabsContent, nameof(tabsContent));
            Assert.IsNotNull(sceneObjectsView, nameof(sceneObjectsView));
            Assert.IsNotNull(trackedObjectsView, nameof(trackedObjectsView));
            Assert.IsNotNull(sceneHierarchyTab, nameof(sceneHierarchyTab));
            Assert.IsNotNull(trackedObjectsTab, nameof(trackedObjectsTab));
            Assert.IsNotNull(toolbar, nameof(toolbar));
        }

        protected override void BindUIElements()
        {
            sceneHierarchyScrollView = Root.Q<ScrollView>("HierarchyScrollView");

            addRoomButton = Root.Q<Button>("AddRoom-button");
            addModelButton = Root.Q<Button>("AddModel-button");
            clearSceneButton = Root.Q<Button>("ClearScene-button");

            propertiesWindow = Root.Q<VisualElement>("Properties-window");
            toolbar = Root.Q<Toolbar>("Toolbar");

            splitView = Root.Q<TwoPaneSplitView>("SplitView");
            tabsContent = Root.Q<VisualElement>("Tabs-content");
            sceneObjectsView = Root.Q<VisualElement>("SceneObjects");
            trackedObjectsView = Root.Q<VisualElement>("TrackedObjects");
            buttons = Root.Q<VisualElement>("Buttons");
            sceneHierarchyTab = Root.Q<ToolbarToggle>("SceneObjectsTab-toggle");
            trackedObjectsTab = Root.Q<ToolbarToggle>("TrackedObjectsTab-toggle");

            propertiesPresenter = new PropertiesPresenter(propertiesWindow);
            sceneObjectsGraph = new HierarchyPresenter(sceneObjectsView, State.OpenedSceneObjectFoldoutsIds);
            trackedObjectsGraph = new HierarchyPresenter(trackedObjectsView);
        }

        protected override void RegisterUICallbacks()
        {
            sceneObjectsGraph.SelectionChanged += selectedNodesId => NodeSelected?.Invoke(selectedNodesId);
            trackedObjectsGraph.SelectionChanged += selectedNodesId => NodeSelected?.Invoke(selectedNodesId);

            sceneObjectsGraph.NodeParentChanged += newParentId => NodeParentChanged?.Invoke(newParentId);
            trackedObjectsGraph.NodeParentChanged += newParentId => NodeParentChanged?.Invoke(newParentId);

            addRoomButton.clicked += () => AddRoomClicked?.Invoke();
            addModelButton.clicked += () => AddModelClicked?.Invoke();
            clearSceneButton.clicked += () => ClearSceneClicked?.Invoke();

            Root.RegisterCallback<KeyUpEvent>(KeyUp);
            Root.RegisterCallback<FocusInEvent>(OnFocusIn);
            tabsContent.RegisterCallback<MouseOverEvent>(AutoScrollWhenDragged);

            sceneHierarchyTab.RegisterValueChangedCallback(e => OnTabChanged(true));
            trackedObjectsTab.RegisterValueChangedCallback(e => OnTabChanged(false));

            tabsContent.RegisterCallback<DragUpdatedEvent>(DragUpdated);
            tabsContent.RegisterCallback<DragPerformEvent>(DragPerformed);
        }

        protected override void Serialize()
        {
            if (State.IsSceneObjectsTabOpened)
            {
                State.SceneObjectsScrollOffset = sceneHierarchyScrollView.scrollOffset;
            }
            else
            {
                State.TrackedObjectsScrollOffset = sceneHierarchyScrollView.scrollOffset;
            }

            base.Serialize();
        }

        protected override void SynchronizeViewWithState()
        {
            SetActiveTab(State.IsSceneObjectsTabOpened);
            propertiesPresenter.SetPropertiesFoldout(State.IsPropertyFoldoutOpened);
        }

        public override void ClearFields()
        {
            propertiesPresenter.ClearFields();
        }

        private void OnFocusIn(FocusInEvent evt)
        {
            if(!Root.panel.visualTree.HasBubbleUpHandlers())
            {
                //  this register could not be called in RegisterUICallbacks
                Root.panel.visualTree.RegisterCallback<KeyDownEvent>(KeyDownInPanel);
                Root.UnregisterCallback<FocusInEvent>(OnFocusIn);
            }
        }

        private void KeyDownInPanel(KeyDownEvent evt) => ActiveHierarchyPresenter.KeyDownInPanel(evt);

        private void AutoScrollWhenDragged(MouseOverEvent evt)
        {
            if (evt.pressedButtons != 1 || !sceneHierarchyScrollView.verticalScroller.visible)
            {
                return;
            }

            if (evt.localMousePosition.y <= 30 && DragAndDrop.GetGenericData(NodeViewPresenter.DraggedNodeKey) != null)
            {
                sceneHierarchyScrollView.scrollOffset -= Vector2.up * 20;
                ActiveHierarchyPresenter.SetSelectedNodes(SelectedNodesID);
            }
            else if (evt.localMousePosition.y >= sceneHierarchyScrollView.contentRect.height - 30 &&
                DragAndDrop.GetGenericData(NodeViewPresenter.DraggedNodeKey) != null)
            {
                sceneHierarchyScrollView.scrollOffset += Vector2.up * 20;
                ActiveHierarchyPresenter.SetSelectedNodes(SelectedNodesID);
            }
        }

        private void DragPerformed(DragPerformEvent evt)
        {
            var node = (SceneGraphNodeData) DragAndDrop.GetGenericData(NodeViewPresenter.DraggedNodeKey);
            NodeParentChanged?.Invoke(string.Empty);
            evt.StopPropagation();
        }

        private void DragUpdated(DragUpdatedEvent evt)
        {
            var node = (SceneGraphNodeData) DragAndDrop.GetGenericData(NodeViewPresenter.DraggedNodeKey);
            DragAndDrop.visualMode = node.HasParent ? DragAndDropVisualMode.Move : DragAndDropVisualMode.Rejected;

            evt.StopPropagation();
        }

        private void KeyUp(KeyUpEvent keyUpEvent)
        {
            if (keyUpEvent.keyCode == KeyCode.Delete)
            {
                DeleteButtonClicked?.Invoke();
            }
        }

        private void OnTabChanged(bool sceneObjectTabClicked)
        {
            State.IsSceneObjectsTabOpened = sceneObjectTabClicked;

            if (State.IsSceneObjectsTabOpened)
            {
                State.TrackedObjectsScrollOffset = sceneHierarchyScrollView.scrollOffset;
                sceneHierarchyScrollView.scrollOffset = State.SceneObjectsScrollOffset;
            }
            else
            {
                State.SceneObjectsScrollOffset = sceneHierarchyScrollView.scrollOffset;
                sceneHierarchyScrollView.scrollOffset = State.TrackedObjectsScrollOffset;
            }

            SetActiveTab(sceneObjectTabClicked);
        }

        private void SetActiveTab(bool isSceneObjectTabOpened)
        {
            sceneHierarchyTab.SetValueWithoutNotify(isSceneObjectTabOpened);
            trackedObjectsTab.SetValueWithoutNotify(!isSceneObjectTabOpened);

            sceneObjectsView.SetDisplay(isSceneObjectTabOpened);
            trackedObjectsView.SetDisplay(!isSceneObjectTabOpened);
        }
    }
}
