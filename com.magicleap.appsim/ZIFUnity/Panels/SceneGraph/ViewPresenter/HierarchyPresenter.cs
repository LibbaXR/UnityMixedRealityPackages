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
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal sealed class HierarchyPresenter : CustomView
    {
        public event Action<string> NodeNameChanged = delegate { };
        public event Action<string> NodeParentChanged = delegate { };
        public event Action<string> NodeDeleted = delegate { };
        public event Action<IReadOnlyList<string>> SelectionChanged = delegate { };

        private readonly List<string> allNodesId = new();
        private readonly List<string> openedSceneObjectFoldoutsIds;
        private readonly Dictionary<string, NodeViewPresenter> renderedNodes = new();
        private readonly List<string> selectedNodes = new();

        private int selectedBlockBegin = -1;
        private int selectedBlockEnd = -1;

        public VisualElement SelectedNode { private set; get; }

        public HierarchyPresenter(VisualElement root, List<string> openedSceneObjectFoldoutsIds = null)
        {
            Root = root;
            this.openedSceneObjectFoldoutsIds = openedSceneObjectFoldoutsIds ?? new List<string>();
            Initialize();
        }

        public void Clear()
        {
            for (int i = Root.childCount - 1; i >= 0; i--)
            {
                Root.RemoveAt(i);
            }

            allNodesId.Clear();
            renderedNodes.Clear();
        }

        public void SetData(IEnumerable<SceneGraphNodeData> data)
        {
            Clear();
            RenderNodes(new List<SceneGraphNodeData>(data));
        }

        public void SetNodeName(string slectedNodeId, string newName)
        {
            if (renderedNodes.TryGetValue(slectedNodeId, out NodeViewPresenter node))
            {
                node.SetNodeName(newName);
            }
        }

        public void SetSelectedNodes(IReadOnlyList<string> selectedNodesId)
        {
            ClearNodesSelection();

            foreach (string nodeId in selectedNodesId)
            {
                if (renderedNodes.TryGetValue(nodeId, out NodeViewPresenter node))
                {
                    selectedNodes.Add(nodeId);

                    node.SetFocus(true);
                    SelectedNode = node.Root;
                }
            }
        }

        internal void KeyDownInPanel(KeyDownEvent keyDownEvent)
        {
            if (keyDownEvent.ctrlKey || keyDownEvent.commandKey)
            {
                if (keyDownEvent.keyCode == KeyCode.A)
                {
                    SelectAll();
                }
            }
        }

        protected override void RegisterUICallbacks()
        {
            base.RegisterUICallbacks();

            Root.RegisterCallback<KeyDownEvent>(KeyDown, TrickleDown.TrickleDown);
        }

        private void SelectAll()
        {
            selectedNodes.Clear();

            selectedBlockBegin = 0;
            selectedBlockEnd = allNodesId.Count > 0 ? allNodesId.Count - 1 : 0;

            selectedNodes.AddRange(GetNodeBlock(selectedBlockBegin, selectedBlockEnd));

            SelectionChanged?.Invoke(selectedNodes);
        }

        private void ClearNodesSelection()
        {
            SelectedNode = null;
            selectedNodes.Clear();

            foreach (NodeViewPresenter nodeViewPresenter in renderedNodes.Values)
            {
                nodeViewPresenter.SetFocus(false);
            }
        }

        private void FoldoutKeyDown(KeyDownEvent evt)
        {
            bool isFoldOutSet = evt.keyCode == KeyCode.RightArrow || evt.keyCode == KeyCode.LeftArrow;
            if (isFoldOutSet)
            {
                bool isOpened = evt.keyCode == KeyCode.RightArrow;

                string selectedNode = selectedNodes.FirstOrDefault();
                if (!string.IsNullOrEmpty(selectedNode))
                {
                    renderedNodes[selectedNode].SetFoldoutState(isOpened, true);
                }
            }
        }

        private ICollection<string> GetNodeBlock(int beginNodeIndex, int endNodeIndex)
        {
            var nodeBlock = new List<string>();

            int beginIndex = Mathf.Min(beginNodeIndex, endNodeIndex);
            int endIndex = Mathf.Max(beginNodeIndex, endNodeIndex);

            for (int i = beginIndex; i <= endIndex; i++)
            {
                nodeBlock.Add(allNodesId[i]);
            }

            return nodeBlock;
        }

        private int GetNodeIndex(string nodeId)
        {
            for (int i = 0; i < allNodesId.Count; i++)
            {
                if (allNodesId[i] == nodeId)
                {
                    return i;
                }
            }

            return 0;
        }

        private void KeyDown(KeyDownEvent evt)
        {
            NavigationKeyDown(evt);
            FoldoutKeyDown(evt);
        }

        private void NavigationKeyDown(KeyDownEvent evt)
        {
            bool isNavigationKeyDown = evt.keyCode == KeyCode.UpArrow || evt.keyCode == KeyCode.DownArrow;
            bool isCtrlDown = evt.ctrlKey;

            if (!isNavigationKeyDown || isCtrlDown)
            {
                return;
            }

            int selectionOffset = evt.keyCode == KeyCode.UpArrow ? 1 : -1;
            int selectedNodeIndex = Mathf.Clamp(selectedBlockEnd - selectionOffset, 0, allNodesId.Count - 1);

            if (evt.shiftKey)
            {
                selectedBlockEnd = selectedNodeIndex;

                selectedNodes.Clear();
                selectedNodes.AddRange(GetNodeBlock(selectedBlockBegin, selectedBlockEnd));
            }
            else
            {
                string newSelection = allNodesId[selectedNodeIndex];

                if (!string.IsNullOrEmpty(newSelection))
                {
                    selectedNodes.Clear();
                    selectedNodes.Add(newSelection);

                    selectedBlockBegin = selectedNodeIndex;
                    selectedBlockEnd = selectedNodeIndex;
                }
            }

            SelectionChanged?.Invoke(selectedNodes);
        }

        private void OnNodeFoldoutToggled(string nodeId, bool isOpened)
        {
            if (renderedNodes.TryGetValue(nodeId, out NodeViewPresenter node))
            {
                node.SetFoldoutState(isOpened);
            }

            if (isOpened && !openedSceneObjectFoldoutsIds.Contains(nodeId))
            {
                openedSceneObjectFoldoutsIds.Add(nodeId);
            }
            else if (!isOpened && openedSceneObjectFoldoutsIds.Contains(nodeId))
            {
                openedSceneObjectFoldoutsIds.Remove(nodeId);
            }
        }

        private void OnNodeSelect(string nodeId, bool isCtrl, bool isShift)
        {
            int selectedNodeIndex = GetNodeIndex(nodeId);

            if (selectedNodes.Count == 0 || !isCtrl && !isShift)
            {
                selectedNodes.Clear();
                selectedNodes.Add(nodeId);

                selectedBlockBegin = selectedNodeIndex;
                selectedBlockEnd = selectedNodeIndex;
            }
            else if (isCtrl)
            {
                if (selectedNodes.Contains(nodeId))
                {
                    selectedNodes.Remove(nodeId);
                }
                else
                {
                    selectedNodes.Add(nodeId);
                }

                selectedBlockBegin = selectedNodeIndex;
                selectedBlockEnd = selectedNodeIndex;
            }
            else if (isShift)
            {
                selectedBlockEnd = selectedNodeIndex;

                selectedNodes.Clear();
                selectedNodes.AddRange(GetNodeBlock(selectedBlockBegin, selectedBlockEnd));
            }

            SelectionChanged?.Invoke(selectedNodes);
        }

        private void OnNodeDeleted(string nodeId)
        {
            NodeDeleted?.Invoke(nodeId);
        }

        private void RenderNodes(IEnumerable<SceneGraphNodeData> list, NodeViewPresenter parent = null)
        {
            if (list == null)
            {
                return;
            }

            foreach (SceneGraphNodeData item in list)
            {
                var nodeViewPresenter = new NodeViewPresenter(parent, item);
                allNodesId.Add(item.Id);
                renderedNodes.Add(item.Id, nodeViewPresenter);

                if (parent == null)
                {
                    Root.Add(nodeViewPresenter.Root);
                }
                else
                {
                    parent.ChildrenContainer.Add(nodeViewPresenter.Root);
                }

                nodeViewPresenter.NameChanged += NodeNameChanged;
                nodeViewPresenter.NodeFocused += OnNodeSelect;
                nodeViewPresenter.NodeParentChanged += NodeParentChanged;
                nodeViewPresenter.NodeDeleted += OnNodeDeleted;
                nodeViewPresenter.SetFoldoutActive(item.HasChildren);

                if (item.HasChildren)
                {
                    nodeViewPresenter.FoldoutStateChanged += OnNodeFoldoutToggled;
                    nodeViewPresenter.SetFoldoutState(openedSceneObjectFoldoutsIds.Contains(item.Id));

                    RenderNodes(item.Children, nodeViewPresenter);
                }
            }
        }
    }
}
