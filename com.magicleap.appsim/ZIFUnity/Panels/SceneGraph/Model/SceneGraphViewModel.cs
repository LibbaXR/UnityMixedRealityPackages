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
using System.Dynamic;
using System.IO;
using ml.zi;
using UnityEditor;
using UnityEngine;
using LightType = ml.zi.LightType;

namespace MagicLeap.ZI
{
    internal class SceneGraphViewModel : ViewModel
    {
        private class Vector3Cached
        {
            private Vector3 newVectorValue;

            public Vector3 Value => newVectorValue;

            public bool VectorChanged => XUpdated || YUpdated || ZUpdated;
            public bool XUpdated { get; private set; }

            public bool YUpdated { get; private set; }

            public bool ZUpdated { get; private set; }

            public void Clear(Vector3 newValue)
            {
                newVectorValue = newValue;
                XUpdated = false;
                YUpdated = false;
                ZUpdated = false;
            }

            public void SetValue(CombinedVector3 combinedVector)
            {
                Vector3 value = combinedVector.vector;

                XUpdated |= combinedVector.isCombinedX && Math.Abs(value.x - newVectorValue.x) > float.Epsilon;
                YUpdated |= combinedVector.isCombinedY && Math.Abs(value.y - newVectorValue.y) > float.Epsilon;
                ZUpdated |= combinedVector.isCombinedZ && Math.Abs(value.z - newVectorValue.z) > float.Epsilon;

                newVectorValue = value;
            }
        }

        public event Action<IReadOnlyList<SceneGraphNodeData>> SceneGraphStructureChanged;
        public event Action<IReadOnlyList<string>> SelectedNodesChanged;
        public event Action<IReadOnlyList<string>> SelectedNodesDataChanged;
        public event Action<IReadOnlyList<SceneGraphNodeData>> TrackedObjectsGraphStructureChanged;
        private const double CacheUpdateFrequency = 1f / 60f;

        private const uint ProgressMonitorTimeout = 10000;
        private readonly Dictionary<string, LightNodeDataBuilder> cachedLightNodeData = new();

        private readonly Dictionary<string, SceneNodeBuilder> cachedNodes = new();
        private readonly Vector3Cached cachedOrientation = new();
        private readonly Vector3Cached cachedPosition = new();
        private readonly Vector3Cached cachedScale = new();
        private readonly Dictionary<string, TransformBuilder> cachedTransformData = new();

        private double timeSinceLastCacheUpdate;

        private ZIBridge.ModuleWrapper<SceneGraph, SceneGraphChanges> SceneGraph => Bridge.SceneGraph;
        private ZIBridge.ModuleWrapper<VirtualRoom, VirtualRoomChanges> VirtualRoom => Bridge.VirtualRoom;

        public void AddModel(string path)
        {
            SceneGraph.Handle.AddModel(path,
                ProgressAsyncMonitorDisplay.Show("Adding " + Path.GetFileName(path) + " Model to the scene"));
            UpdateChangedModelValues(SceneGraphChanges.All);
        }

        public void AddRoom(string path)
        {
            SceneGraph.Handle.AddRoom(path,
                ProgressAsyncMonitorDisplay.Show("Adding " + Path.GetFileName(path) + " Room to the scene"));
            UpdateChangedModelValues(SceneGraphChanges.All);
        }

        public void ChangeNodeParent(string nodeId, string newParentId)
        {
            SceneGraph.Handle.ReparentSceneNode(nodeId, newParentId, true, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
        }

        public void ClearScene()
        {
            if (EditorUtility.DisplayDialog("Clear Scene",
                "Clearing the current scene cannot be undone. Are you sure you want to continue?",
                "Ok",
                "Cancel"))
            {
                SceneGraph.Handle.ClearScene();
            }
        }

        public void DeleteNode(string nodeId)
        {
            SceneGraph.Handle.DestroySceneNodeById (nodeId, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
        }
        
        public void DeleteNodes(IReadOnlyList<string> selectedNodesId)
        {
            StringList nodesList = StringList.Alloc();
            StringList invalidNodesList = StringList.Alloc();

            foreach (string nodeId in selectedNodesId)
            {
                nodesList.Append(nodeId);
            }

            SceneGraph.Handle.DestroySceneNodesById(nodesList, invalidNodesList, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
            nodesList.Dispose();
            invalidNodesList.Dispose();
        }

        public Color GetNodeLightColor(string nodeId)
        {
            return GetLightData(nodeId).LightColor.ToColor();
        }

        public float GetNodeLightConeAngle(string nodeId)
        {
            return GetLightData(nodeId).LightConeAngle;
        }

        public float GetNodeLightIntensity(string nodeId)
        {
            return GetLightData(nodeId).LightIntensity;
        }

        public float GetNodeLightRange(string nodeId)
        {
            return GetLightData(nodeId).LightRange;
        }

        public LightType GetNodeLightType(string nodeId)
        {
            return GetLightData(nodeId).LightType;
        }

        public string GetNodeName(string nodeId)
        {
            return GetNode(nodeId).Name;
        }

        public Vector3 GetNodeOrientation(string nodeId)
        {
            Vector3 orientation = GetTransform(nodeId).EulerRotation.ToVec3();
            cachedOrientation.Clear(orientation);
            return orientation;
        }

        public Vector3 GetNodePosition(string nodeId)
        {
            Vector3 position = GetTransform(nodeId).Position.ToVec3();
            cachedPosition.Clear(position);
            return position;
        }

        public Vector3 GetNodeScale(string nodeId)
        {
            Vector3 scale = GetTransform(nodeId).Scale.ToVec3();
            cachedScale.Clear(scale);
            return scale;
        }

        public SceneNodeType GetNodeType(string nodeId)
        {
            return GetNode(nodeId).NodeType;
        }

        public IReadOnlyList<SceneGraphNodeData> GetSceneObjectsNode()
        {
            var sceneObjects = new List<SceneGraphNodeData>();

            SceneNodeList list = SceneNodeList.Alloc();
            SceneGraph.Handle.GetSceneGraph(SceneGraphType.Room, list, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));

            uint sceneObjectsCount = list.GetSize();

            for (uint i = 0; i < sceneObjectsCount; i++)
            {
                sceneObjects.Add(MapViewSceneNode(list.Get(i), null));
            }

            list.Dispose();
            return sceneObjects;
        }

        public IReadOnlyList<string> GetSelectedNodes()
        {
            StringList selectedNodes = StringList.Alloc();
            SceneGraph.Handle.GetSelectedSceneNodes(selectedNodes);

            var selectedNodesId = new List<string>();

            for (uint i = 0; i < selectedNodes.GetSize(); i++)
            {
                selectedNodesId.Add(selectedNodes.Get(i));
            }

            selectedNodes.Dispose();

            return selectedNodesId;
        }

        public IReadOnlyList<SceneGraphNodeData> GetTrackedObjectsNode()
        {
            var trackedObjects = new List<SceneGraphNodeData>();

            SceneNodeList list = SceneNodeList.Alloc();
            SceneGraph.Handle.GetSceneNodesOfType(SceneNodeType.ImageTargetGizmo, list, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));

            uint sceneObjectsCount = list.GetSize();

            for (uint i = 0; i < sceneObjectsCount; i++)
            {
                SceneNodeBuilder node = list.Get(i);

                SceneGraphNodeData viewSceneObjectNode = new(node.GetNodeId(), node.GetName(), node.GetNodeType(), null);

                trackedObjects.Add(viewSceneObjectNode);
            }

            list.Dispose();

            return trackedObjects;
        }

        public override void Initialize()
        {
            SceneGraph.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            VirtualRoom.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
            SceneGraph.OnTakeChanges += UpdateChangedModelValues;
            VirtualRoom.OnTakeChanges += VirtualRoomChanged;
        }
        
        public override void UnInitialize()
        {
            SceneGraph.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            VirtualRoom.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
            SceneGraph.OnTakeChanges -= UpdateChangedModelValues;
            VirtualRoom.OnTakeChanges -= VirtualRoomChanged;
        }

        public void SetNodeLightData(string nodeId, LightPropertiesData lightPropertiesData)
        {
            LightNodeDataBuilder lightData = LightNodeDataBuilder.Alloc();

            lightData.SetLightColor(lightPropertiesData.Color.ToVec3f());
            lightData.SetLightType(lightPropertiesData.Type);
            lightData.SetLightIntensity(lightPropertiesData.Intensity);
            lightData.SetLightRange(lightPropertiesData.Range);
            lightData.SetLightConeAngle(lightPropertiesData.ConeAngle);

            SceneGraph.Handle.SetLightNodeData(nodeId, lightData, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));

            lightData.Dispose();
        }

        public void SetNodeName(string nodeId, string newName)
        {
            SceneGraph.Handle.SetSceneNodeName(nodeId, newName, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
        }

        public void SetSelectedNodeOrientation(CombinedVector3 newOrientation)
        {
            cachedOrientation.SetValue(newOrientation);
        }

        public void SetSelectedNodePosition(CombinedVector3 newPosition)
        {
            cachedPosition.SetValue(newPosition);
        }

        public void SetSelectedNodes(IReadOnlyList<string> selectedNodesId)
        {
            StringList nodesList = StringList.Alloc();
            StringList invalidNodesList = StringList.Alloc();

            foreach (string nodeId in selectedNodesId)
            {
                nodesList.Append(nodeId);
            }

            SceneGraph.Handle.SelectSceneNodesById(nodesList, true, invalidNodesList, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
            nodesList.Dispose();
            invalidNodesList.Dispose();
        }

        public void SetSelectedNodeScale(CombinedVector3 newScale)
        {
            cachedScale.SetValue(newScale);
        }

        public override void Update()
        {
            if (IsSessionRunning)
            {
                UpdateCachedValues();
            }
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && Bridge.SceneGraph.IsHandleConnected && Bridge.VirtualRoom.IsHandleConnected;
        }

        private void ClearCachedNodes()
        {
            foreach (SceneNodeBuilder node in cachedNodes.Values)
            {
                node.Dispose();
            }

            cachedNodes.Clear();

            foreach (TransformBuilder transformBuilder in cachedTransformData.Values)
            {
                transformBuilder.Dispose();
            }

            cachedTransformData.Clear();

            foreach (LightNodeDataBuilder lightData in cachedLightNodeData.Values)
            {
                lightData.Dispose();
            }

            cachedLightNodeData.Clear();
        }

        private LightNodeDataBuilder GetLightData(string nodeId)
        {
            LightNodeDataBuilder lightData;
            if (cachedLightNodeData.TryGetValue(nodeId, out lightData))
            {
                return lightData;
            }

            lightData = GetNode(nodeId).LightNodeData;
            cachedLightNodeData[nodeId] = lightData;
            return lightData;
        }

        private SceneNodeBuilder GetNode(string nodeId)
        {
            if (cachedNodes.TryGetValue(nodeId, out SceneNodeBuilder node))
            {
                return node;
            }

            SceneNodeBuilder newNode = SceneNodeBuilder.Alloc();
            SceneGraph.Handle.GetSceneNodeById(nodeId, newNode, false, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
            cachedNodes[nodeId] = newNode;

            return newNode;
        }

        private TransformBuilder GetTransform(string nodeId)
        {
            TransformBuilder transform;
            if (cachedTransformData.TryGetValue(nodeId, out transform))
            {
                return transform;
            }

            transform = GetNode(nodeId).Transform;
            cachedTransformData[nodeId] = transform;
            return transform;
        }

        private SceneGraphNodeData MapViewSceneNode(SceneNodeBuilder node, SceneGraphNodeData parent)
        {
            var viewSceneObjectNode =
                new SceneGraphNodeData(node.GetNodeId(), node.GetName(), node.GetNodeType(), parent);

            SceneNodeList children = node.Children;
            uint childrenCount = children == null ? 0 : children.GetSize();

            for (uint i = 0; i < childrenCount; i++)
            {
                SceneNodeBuilder child = children.Get(i);
                viewSceneObjectNode.Children.Add(MapViewSceneNode(child, viewSceneObjectNode));
            }

            return viewSceneObjectNode;
        }

        private void SetSelectedNodeOrientation()
        {
            SceneGraph.Handle.SetSelectionEulerRotationPerAxis(cachedOrientation.Value.ToVec3f(), cachedOrientation.XUpdated,
                cachedOrientation.YUpdated, cachedOrientation.ZUpdated, false, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
        }

        private void SetSelectedNodePosition()
        {
            SceneGraph.Handle.SetSelectionPositionPerAxis(cachedPosition.Value.ToVec3f(), cachedPosition.XUpdated, cachedPosition.YUpdated,
                cachedPosition.ZUpdated, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
        }

        private void SetSelectedNodeScale()
        {
            SceneGraph.Handle.SetSelectionScalePerAxis(cachedScale.Value.ToVec3f(), cachedScale.XUpdated, cachedScale.YUpdated,
                cachedScale.ZUpdated, ProgressMonitor.AllocWithTimeout(ProgressMonitorTimeout));
        }

        private void UpdateCachedValues()
        {
            if (!(EditorApplication.timeSinceStartup - timeSinceLastCacheUpdate >= CacheUpdateFrequency))
            {
                return;
            }

            timeSinceLastCacheUpdate = EditorApplication.timeSinceStartup;

            if (cachedPosition.VectorChanged)
            {
                SetSelectedNodePosition();
            }

            if (cachedOrientation.VectorChanged)
            {
                SetSelectedNodeOrientation();
            }

            if (cachedScale.VectorChanged)
            {
                SetSelectedNodeScale();
            }
        }

        private void RefreshSceneGraph()
        {
            ClearCachedNodes();

            SceneGraphStructureChanged?.Invoke(GetSceneObjectsNode());
            TrackedObjectsGraphStructureChanged?.Invoke(GetTrackedObjectsNode());
        }
        
        private void VirtualRoomChanged(VirtualRoomChanges virtualRoomChanges)
        {
            if (virtualRoomChanges.HasFlag(VirtualRoomChanges.SessionLoadCompleted))
            {
                RefreshSceneGraph();
            }
        }
        
        private void UpdateChangedModelValues(SceneGraphChanges changes)
        {
            if (changes.HasFlag(SceneGraphChanges.SceneCleared) ||
                changes.HasFlag(SceneGraphChanges.SessionConnected) ||
                changes.HasFlag(SceneGraphChanges.SceneNodesDestroyed) ||
                changes.HasFlag(SceneGraphChanges.SceneNodesModified))
            {
                RefreshSceneGraph();
            }

            IReadOnlyList<string> selectedNodesId = GetSelectedNodes();

            if (changes.HasFlag(SceneGraphChanges.SelectionChanged))
            {
                SelectedNodesChanged?.Invoke(selectedNodesId);
            }

            if (changes.HasFlag(SceneGraphChanges.SelectionDataChanged))
            {
                ClearCachedNodes();
                SelectedNodesDataChanged?.Invoke(selectedNodesId);
            }
        }
    }
}
