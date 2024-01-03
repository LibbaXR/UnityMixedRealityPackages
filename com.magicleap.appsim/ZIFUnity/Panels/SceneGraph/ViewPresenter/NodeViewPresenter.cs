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
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using MouseButton = UnityEngine.UIElements.MouseButton;

namespace MagicLeap.ZI
{
    internal class NodeViewPresenter : CustomView
    {
        private enum DragState
        {
            AtRest,
            Ready,
            Dragging
        }

        public event Action<string, bool> FoldoutStateChanged = delegate { };
        public event Action<string> NameChanged = delegate { };
        public event Action<string, bool, bool> NodeFocused = delegate { };
        public event Action<string> NodeParentChanged = delegate { };
        public event Action<string> NodeDeleted = delegate { };

        public const string DraggedNodeKey = "NodeData";

        private const string ViewPath = "Packages/com.magicleap.appsim/ZIFUnity/Panels/SceneGraph/Views/ZISceneGraphNodeView.uxml";

        private readonly SceneGraphNodeData nodeData;

        private readonly NodeViewPresenter parent;
        private DragState dragState;
        private Toggle foldout;

        private VisualElement header;
        private VisualElement icon;
        private VisualElement label;
        private Label nameLabel;
        private TextField nameTextField;

        private float timeSinceLastHover = float.MaxValue;

        public VisualElement ChildrenContainer { get; private set; }

        private bool IsSelected => label.ClassListContains("selected");

        public NodeViewPresenter(NodeViewPresenter parent, SceneGraphNodeData nodeData)
        {
            this.parent = parent;
            this.nodeData = nodeData;

            var template = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(ViewPath);
            Root = template.Instantiate();

            Initialize();
        }

        public void SetFocus(bool isFocused)
        {
            if (isFocused)
            {
                label.AddToClassList("selected");
                UnFoldParent();
            }
            else
            {
                label.RemoveFromClassList("selected");
            }
        }

        public void SetFoldoutActive(bool isActive)
        {
            foldout.style.visibility = isActive ? Visibility.Visible : Visibility.Hidden;
        }

        public void SetFoldoutState(bool isOpened, bool shouldNotify = false)
        {
            if (shouldNotify)
            {
                foldout.value = isOpened;
            }
            else
            {
                foldout.SetValueWithoutNotify(isOpened);
            }

            ChildrenContainer.SetDisplay(isOpened);
        }

        public void SetNodeName(string name)
        {
            nameLabel.text = name;
        }

        protected override void AssertFields()
        {
            Assert.IsNotNull(header, nameof(header));
            Assert.IsNotNull(ChildrenContainer, nameof(ChildrenContainer));
            Assert.IsNotNull(foldout, nameof(foldout));
            Assert.IsNotNull(nameLabel, nameof(nameLabel));
            Assert.IsNotNull(icon, nameof(icon));
            Assert.IsNotNull(nameTextField, nameof(nameTextField));
            Assert.IsNotNull(label, nameof(label));
        }

        protected override void BindUIElements()
        {
            header = Root.Q("Header");
            ChildrenContainer = Root.Q("ChildrenContainer");
            foldout = Root.Q<Toggle>("toggle");
            nameLabel = Root.Q<Label>("Name-label");
            icon = Root.Q<VisualElement>("Icon-texture");
            nameTextField = Root.Q<TextField>("Name-textField");
            label = Root.Q<VisualElement>("Label");
        }

        protected override void RegisterUICallbacks()
        {
            foldout.RegisterValueChangedCallback(e => FoldoutStateChanged?.Invoke(nodeData.Id, e.newValue));
            nameTextField.RegisterCallback<ChangeEvent<string>>(OnNameChanged);
            nameTextField.RegisterCallback<FocusOutEvent>(e => DisableNameEdit());

            header.RegisterCallback<MouseDownEvent>(MouseDown);
            header.RegisterCallback<MouseUpEvent>(MouseUp);
            header.RegisterCallback<MouseMoveEvent>(MouseMoved);

            header.RegisterCallback<DragUpdatedEvent>(DragUpdated);
            header.RegisterCallback<DragPerformEvent>(DragPerformed);
            header.RegisterCallback<DragEnterEvent>(DragEntered);
            header.RegisterCallback<DragLeaveEvent>(DragLeft);

            ChildrenContainer.RegisterCallback<DragPerformEvent>(DragPerformed);
            ChildrenContainer.RegisterCallback<DragUpdatedEvent>(DragUpdated);
        }

        protected override void SynchronizeViewWithState()
        {
            nameLabel.text = nodeData.Name;
            nameTextField.isDelayed = true;

            icon.style.backgroundImage = nodeData.Icon;
        }

        private bool CanBeDropped(SceneGraphNodeData droppedNode)
        {
            if (droppedNode == null ||
                droppedNode.Id == nodeData.Id ||
                IsSelected || nodeData.Type == SceneNodeType.ImageTargetGizmo)
            {
                return false;
            }

            if (droppedNode.Parent != null && droppedNode.Parent.Id == nodeData.Id)
            {
                return false;
            }

            return droppedNode.Children.All(CanBeDropped);
        }

        private void DisableNameEdit()
        {
            nameTextField.SetDisplay(false);
            nameLabel.SetDisplay(true);
        }

        private void DragEntered(DragEnterEvent evt)
        {
            if (CanBeDropped((SceneGraphNodeData) DragAndDrop.GetGenericData(DraggedNodeKey)))
            {
                SetHoverable(true);
                timeSinceLastHover = Time.realtimeSinceStartup;
            }

            evt.StopPropagation();
        }

        private void DragLeft(DragLeaveEvent evt)
        {
            if (CanBeDropped((SceneGraphNodeData) DragAndDrop.GetGenericData(DraggedNodeKey)))
            {
                SetHoverable(false);
            }

            timeSinceLastHover = float.MaxValue;
            evt.StopPropagation();
        }

        private void DragPerformed(DragPerformEvent evt)
        {
            SetFocus(false);
            timeSinceLastHover = float.MaxValue;
            NodeParentChanged?.Invoke(nodeData.Id);
            evt.StopPropagation();
        }

        private void DragUpdated(DragUpdatedEvent evt)
        {
            if (CanBeDropped((SceneGraphNodeData) DragAndDrop.GetGenericData(DraggedNodeKey)))
            {
                if (Time.realtimeSinceStartup - timeSinceLastHover > 0.5f)
                {
                    foldout.value = true;
                }

                DragAndDrop.visualMode = DragAndDropVisualMode.Link;
            }
            else
            {
                DragAndDrop.visualMode = DragAndDropVisualMode.Rejected;
            }

            evt.StopPropagation();
        }

        private void EnableNameEdit()
        {
            nameTextField.SetDisplay(true);
            nameLabel.SetDisplay(false);

            nameTextField.SetValueWithoutNotify(nameLabel.text);

            nameTextField.Q("unity-text-input").Focus();
            nameTextField.SelectAll();
        }

        private void MouseDown(MouseDownEvent evt)
        {
            if (evt.button == (int) MouseButton.LeftMouse)
            {
                dragState = DragState.Ready;
            }

            if (evt.button == (int) MouseButton.LeftMouse && evt.clickCount == 2)
            {
                EnableNameEdit();
                evt.StopPropagation();
            }
            else if (evt.button == (int) MouseButton.RightMouse)
            {
                NodeFocused?.Invoke(nodeData.Id, evt.IncrementalSelectionKey(), evt.shiftKey);

                var contextMenu = new GenericMenu();
                contextMenu.AddItem(new GUIContent("Copy UUID", nodeData.Id), false,
                    () => GUIUtility.systemCopyBuffer = nodeData.Id);
                contextMenu.AddItem(new GUIContent("Rename"), false, EnableNameEdit);
                contextMenu.AddItem(new GUIContent("Delete"), false, DeleteNode);
                contextMenu.ShowAsContext();

                evt.StopPropagation();
            }
        }

        private void DeleteNode()
        {
            NodeDeleted?.Invoke(nodeData.Id);
        }

        private void MouseMoved(MouseMoveEvent evt)
        {
            if (dragState == DragState.Ready && evt.pressedButtons == 1 && nodeData.Type != SceneNodeType.ImageTargetGizmo)
            {
                DragAndDrop.PrepareStartDrag();
                DragAndDrop.SetGenericData(DraggedNodeKey, nodeData);
                DragAndDrop.StartDrag("");
                dragState = DragState.Dragging;
                if (!IsSelected)
                {
                    NodeFocused?.Invoke(nodeData.Id, evt.IncrementalSelectionKey(), evt.shiftKey);
                }

                evt.StopPropagation();
            }

            if (dragState == DragState.AtRest)
            {
                SetHoverable(false);
            }
        }

        private void MouseUp(MouseUpEvent evt)
        {
            if (evt.button == (int) MouseButton.LeftMouse && evt.clickCount == 1)
            {
                NodeFocused?.Invoke(nodeData.Id, evt.IncrementalSelectionKey(), evt.shiftKey);
                evt.StopPropagation();
            }

            if (dragState == DragState.Ready && evt.button == 0)
            {
                dragState = DragState.AtRest;
            }
        }

        private void OnNameChanged(ChangeEvent<string> evt)
        {
            NameChanged?.Invoke(evt.newValue);
            DisableNameEdit();
        }

        private void SetHoverable(bool isHoverable)
        {
            if (isHoverable)
            {
                label.AddToClassList("hoverable");
            }
            else
            {
                label.RemoveFromClassList("hoverable");
            }
        }

        private void UnFoldParent()
        {
            if (parent == null)
            {
                return;
            }

            parent.SetFoldoutState(true);
            parent.UnFoldParent();
        }
    }
}
