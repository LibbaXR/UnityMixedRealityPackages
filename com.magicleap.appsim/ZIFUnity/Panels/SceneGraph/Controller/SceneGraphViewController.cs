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
using System.IO;
using System.Threading.Tasks;
using ml.zi;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class SceneGraphViewController : ViewController<SceneGraphViewModel, SceneGraphViewPresenter>
    {
        private static readonly string windowName = "App Sim Scene Graph";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Scene Graph #F7", false, MenuItemPriority_SceneGraph)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Scene Graph", isValidateFunction: false, priority: MenuItemPriority_SceneGraph)]
#endif
        public static void ShowWindow()
        {
            GetWindow<SceneGraphViewController>(windowName);
        }

        protected void OnDisable()
        {
            Presenter.AddRoomClicked -= AddRoomClicked;
            Presenter.AddModelClicked -= AddModelClicked;
            Presenter.ClearSceneClicked -= Model.ClearScene;
            Presenter.NodeSelected -= Model.SetSelectedNodes;
            Presenter.NodeNameChanged -= NodeNameChanged;
            Presenter.DeleteNodeClicked -= Model.DeleteNode;
            Presenter.NodePositionChanged -= Model.SetSelectedNodePosition;
            Presenter.NodeOrientationChanged -= Model.SetSelectedNodeOrientation;
            Presenter.NodeScaleChanged -= Model.SetSelectedNodeScale;
            Presenter.DeleteButtonClicked -= DeleteButtonClicked;
            Presenter.NodeParentChanged -= NodeParentChanged;
            Presenter.OnLightDataChanged -= OnNodeLightDataChanged;
            Presenter.OnDisable();
            
            Presenter.SetEnabled(false);

            Model.OnSessionStarted -= OnSessionStarted;
            Model.OnSessionStopped -= OnSessionStopped;
            Model.SceneGraphStructureChanged -= Presenter.SetSceneObjectsGraph;
            Model.TrackedObjectsGraphStructureChanged -= Presenter.SetTrackedObjectsGraph;
            Model.SelectedNodesChanged -= OnSelectedNodesUpdated;
            Model.SelectedNodesDataChanged -= OnSelectedNodesDataChanged;
            Model.UnInitialize();
        }

        protected override void Initialize()
        {
            Presenter.OnEnable(rootVisualElement);
            
            Presenter.AddRoomClicked += AddRoomClicked;
            Presenter.AddModelClicked += AddModelClicked;
            Presenter.ClearSceneClicked += Model.ClearScene;
            Presenter.NodeSelected += Model.SetSelectedNodes;
            Presenter.NodeNameChanged += NodeNameChanged;
            Presenter.DeleteNodeClicked += Model.DeleteNode;
            Presenter.NodePositionChanged += Model.SetSelectedNodePosition;
            Presenter.NodeOrientationChanged += Model.SetSelectedNodeOrientation;
            Presenter.NodeScaleChanged += Model.SetSelectedNodeScale;
            Presenter.DeleteButtonClicked += DeleteButtonClicked;
            Presenter.NodeParentChanged += NodeParentChanged;
            Presenter.OnLightDataChanged += OnNodeLightDataChanged;

            Presenter.SetEnabled(false);

            Model.OnSessionStarted += OnSessionStarted;
            Model.OnSessionStopped += OnSessionStopped;
            Model.SceneGraphStructureChanged += Presenter.SetSceneObjectsGraph;
            Model.TrackedObjectsGraphStructureChanged += Presenter.SetTrackedObjectsGraph;
            Model.SelectedNodesChanged += OnSelectedNodesUpdated;
            Model.SelectedNodesDataChanged += OnSelectedNodesDataChanged;
            base.Initialize();
        }

        private void AddModelClicked()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Add Model", Settings.Instance.ModelsPath, new[]
                {
                    "All Supported Formats", "fbx,FBX,dae,DAE,blend,3ds,3DS,ase,ASE,obj,OBJ,ply,PLY,gltf,glb",
                    "Autodesk", "fbx,FBX",
                    "Collada", "dae,DAE",
                    "Blender 3D", "blend",
                    "3ds Max 3DS", "3ds,3DS",
                    "3ds Max ASE", "ase,ASE",
                    "Wavefront Object", "obj,OBJ",
                    "Stanford Polygon Library", "ply,PLY",
                    "Khronos glTF (binary)", "gltf",
                    "Any", "*"
                }
            );
            if (!string.IsNullOrWhiteSpace(path))
            {
                Settings.Instance.ModelsPath = Path.GetDirectoryName(path);
                Task.Run(() => Model.AddModel(path));
            }
        }

        private void AddRoomClicked()
        {
            string path = EditorUtility.OpenFilePanelWithFilters("Add Room", Settings.Instance.ExampleVirtualRoomsPath,
                new[]
                {
                    "Virtual Room", "room", "Any", "*"
                }
            );
            if (!string.IsNullOrWhiteSpace(path))
            {
                Task.Run(() => Model.AddRoom(path));
            }
        }

        private void DeleteButtonClicked()
        {
            Model.DeleteNodes(Presenter.SelectedNodesID);
        }

        private void MultipleNodesSelected(IReadOnlyList<string> selectedNodesId)
        {
            PropertiesViewData multiSelectionProperties = PropertiesViewData.incompatiblePropertiesData;

            int nodesCount = selectedNodesId.Count;

            CombinedVector3 position = new(Model.GetNodePosition(selectedNodesId[0]));
            CombinedVector3 orientation = new(Model.GetNodeOrientation(selectedNodesId[0]));
            CombinedVector3 scale = new(Model.GetNodeScale(selectedNodesId[0]));

            SceneNodeType nodeType = Model.GetNodeType(selectedNodesId[0]);
            bool scaleEnabled = nodeType != SceneNodeType.LightNode && nodeType != SceneNodeType.ImageTargetGizmo;

            for (int i = 1; i < nodesCount; i++)
            {
                position.Combine(Model.GetNodePosition(selectedNodesId[i]));
                orientation.Combine(Model.GetNodeOrientation(selectedNodesId[i]));
                scale.Combine(Model.GetNodeScale(selectedNodesId[i]));

                nodeType = Model.GetNodeType(selectedNodesId[i]);
                scaleEnabled &= nodeType != SceneNodeType.LightNode && nodeType != SceneNodeType.ImageTargetGizmo;
            }

            multiSelectionProperties.Position.value = position;
            multiSelectionProperties.Position.enabled = true;
            multiSelectionProperties.Orientation.value = orientation;
            multiSelectionProperties.Orientation.enabled = true;
            multiSelectionProperties.Scale.value = scale;
            multiSelectionProperties.Scale.enabled = scaleEnabled;

            Presenter.SetPropertiesData(multiSelectionProperties);
        }

        private void NodeNameChanged(string newNodeName)
        {
            foreach (string selectedNode in Presenter.SelectedNodesID)
            {
                Model.SetNodeName(selectedNode, newNodeName);
            }
        }

        private void NodeParentChanged(string parentId)
        {
            foreach (string nodeId in Presenter.SelectedNodesID)
            {
                Model.ChangeNodeParent(nodeId, parentId);
            }
        }

        private void OnNodeLightDataChanged(LightPropertiesData lightData)
        {
            foreach (string selectedNode in Presenter.SelectedNodesID)
            {
                Model.SetNodeLightData(selectedNode, lightData);
            }
        }

        private void OnSelectedNodesDataChanged(IReadOnlyList<string> selectedNodeId)
        {
            if (selectedNodeId.Count == 1)
            {
                Presenter.SetNodeName(selectedNodeId[0], Model.GetNodeName(selectedNodeId[0]));
            }

            OnSelectedNodesUpdated(selectedNodeId);
        }

        private void OnSelectedNodesUpdated(IReadOnlyList<string> selectedNodesId)
        {
            Presenter.SetSelectedNodes(selectedNodesId);

            if (selectedNodesId.Count == 0)
            {
                Presenter.SetPropertiesData(PropertiesViewData.emptyPropertiesData);
            }
            else if (selectedNodesId.Count == 1)
            {
                SingleNodeSelected(selectedNodesId[0]);
            }
            else if (selectedNodesId.Count > 1)
            {
                MultipleNodesSelected(selectedNodesId);
            }
        }

        private void OnSessionStarted()
        {
            Presenter.SetEnabled(true);
            Presenter.SetSceneObjectsGraph(Model.GetSceneObjectsNode());
            Presenter.SetTrackedObjectsGraph(Model.GetTrackedObjectsNode());
            OnSelectedNodesUpdated(Model.GetSelectedNodes());
        }

        private void OnSessionStopped()
        {
            Presenter.SetEnabled(false);
            Presenter.ClearSceneGraph();
            Presenter.ClearTrackedObjectsGraph();
            Presenter.ClearListOfActiveFoldouts();
        }

        private void SingleNodeSelected(string selectedNodeId)
        {
            if (string.IsNullOrEmpty(selectedNodeId))
            {
                return;
            }

            SceneNodeType nodeType = Model.GetNodeType(selectedNodeId);
            bool isLightNode = nodeType == SceneNodeType.LightNode;
            bool isScaleEnabled = nodeType != SceneNodeType.LightNode && nodeType != SceneNodeType.ImageTargetGizmo;

            LightPropertiesData lightProperties;

            if (isLightNode)
            {
                lightProperties = new LightPropertiesData
                {
                    Type = Model.GetNodeLightType(selectedNodeId),
                    Color = Model.GetNodeLightColor(selectedNodeId),
                    Intensity = Model.GetNodeLightIntensity(selectedNodeId),
                    Range = Model.GetNodeLightRange(selectedNodeId),
                    ConeAngle = Model.GetNodeLightConeAngle(selectedNodeId)
                };
            }
            else
            {
                lightProperties = LightPropertiesData.empty;
            }

            PropertiesViewData propertiesViewData = new()
            {
                DisplayedType = new PropertyData<string>(nodeType.ToString()),
                Name = new PropertyData<string>(Model.GetNodeName(selectedNodeId)),

                Position = new PropertyData<CombinedVector3>(new CombinedVector3(Model.GetNodePosition(selectedNodeId))),
                Orientation = new PropertyData<CombinedVector3>(new CombinedVector3(Model.GetNodeOrientation(selectedNodeId))),
                Scale = new PropertyData<CombinedVector3>(new CombinedVector3(Model.GetNodeScale(selectedNodeId), isScaleEnabled)),

                Light = new PropertyData<LightPropertiesData>(lightProperties, isLightNode)
            };

            Presenter.SetPropertiesData(propertiesViewData);
        }
    }
}
