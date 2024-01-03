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
using System.IO;
using ml.zi;
using UnityEditor;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class SceneRenderViewModel : RenderViewModel
    {
        public event Action<string, bool> OnGizmoFlagChanged;
        public Action<AnchorMode> OnAnchorModeChanged;
        public Action<SceneViewCameraMode> OnCameraModeChanged;

        public Action<ManipulationMode> OnManipulationModeChanged;
        public Action<ReferenceMode> OnReferenceModeChanged;

        private readonly Dictionary<string, SceneGizmo> gizmoSettings = new();
        private readonly Dictionary<string, SceneGizmo> modifiedGizmos = new();
        private string deviceName;
        public override PeripheralInputSource InputSource => PeripheralInputSource.SceneView;
        public string PreviousModelDirectory { get; private set; }

        public string PreviousRoomDirectory { get; private set; }
        private ZIBridge.ModuleWrapper<ConfigurationSettings, ConfigurationSettingsChanges> ConfigurationSettings => Bridge.ConfigurationSettings;
        private ZIBridge.ModuleWrapper<SceneGraph, SceneGraphChanges> SceneGraph => Bridge.SceneGraph;
        private ZIBridge.ModuleWrapper<VirtualRoom, VirtualRoomChanges> VirtualRoom => Bridge.VirtualRoom;

        public void Add3DModel(string modelPath)
        {
            if (modelPath.Length != 0)
            {
                ReturnedResultStringString addModelResult = SceneGraph.Handle.AddModel(modelPath,
                    ProgressAsyncMonitorDisplay.Show("Adding " + Path.GetFileName(modelPath) + " to scene"));
                if (ZIFGen.ResultIsError(addModelResult.first))
                {
                    Debug.LogError("Error adding model " + modelPath + " to Magic Leap App Simulator virtual scene: " +
                        addModelResult.first);
                }
                else
                {
                    PreviousModelDirectory = Path.GetDirectoryName(modelPath);
                }
            }
        }

        public void AddGizmo(SceneGizmo gizmo)
        {
            if (!gizmoSettings.TryGetValue(gizmo.Name, out _))
            {
                gizmoSettings.Add(gizmo.Name, gizmo);
            }
        }

        public void AddRoom(string roomPath)
        {
            if (roomPath.Length != 0)
            {
                ReturnedResultStringString addRoomResult = SceneGraph.Handle.AddRoom(roomPath,
                    ProgressAsyncMonitorDisplay.Show("Adding " + Path.GetFileName(roomPath) + " to scene"));
                if (ZIFGen.ResultIsError(addRoomResult.first))
                {
                    Debug.LogError("Error adding room " + roomPath + " to Magic Leap App Simulator virtual scene: " +
                        addRoomResult.first);
                }
                else
                {
                    PreviousRoomDirectory = Path.GetDirectoryName(roomPath);
                }
            }
        }

        public void ClearGizmos()
        {
            gizmoSettings.Clear();
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

        public IEnumerable<SceneGizmo> GetAllGizmos()
        {
            var gizmoSettings = new List<SceneGizmo>();

            StringList inGizmoFilterSettings = StringList.Alloc();
            deviceName = VirtualRoom.Handle.GetGizmoFilterSettings(inGizmoFilterSettings);
            uint gizmosCount = inGizmoFilterSettings.GetSize();
            for (uint i = 0; i < gizmosCount; ++i)
            {
                gizmoSettings.Add(GetGizmo(inGizmoFilterSettings.Get(i)));
            }

            inGizmoFilterSettings.Dispose();

            return gizmoSettings;
        }

        public AnchorMode GetAnchorMode()
        {
            return SceneGraph.Handle.GetAnchorMode();
        }

        public SceneViewCameraMode GetCameraMode()
        {
            return VirtualRoom.Handle.GetSceneViewCameraMode();
        }

        public ManipulationMode GetManipulationMode()
        {
            return SceneGraph.Handle.GetManipulationMode();
        }

        public ReferenceMode GetReferenceMode()
        {
            return SceneGraph.Handle.GetReferenceMode();
        }

        public override void Initialize()
        {
            ConfigurationSettings.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            SceneGraph.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            VirtualRoom.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
            ConfigurationSettings.OnTakeChanges += OnConfigurationSettingsChanged;
            SceneGraph.OnTakeChanges += OnSceneGraphChanged;
            VirtualRoom.OnTakeChanges += OnVirtualRoomChanged;
        }
        
        public override void UnInitialize()
        {
            ConfigurationSettings.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            SceneGraph.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            VirtualRoom.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
            ConfigurationSettings.OnTakeChanges -= OnConfigurationSettingsChanged;
            SceneGraph.OnTakeChanges -= OnSceneGraphChanged;
            VirtualRoom.OnTakeChanges -= OnVirtualRoomChanged;
        }

        public void ResetCamera()
        {
            VirtualRoom.Handle.ResetSceneViewCamera();
        }

        public void SetAnchorMode(AnchorMode value)
        {
            SceneGraph.Handle.SetAnchorMode(value);
        }

        public void SetCameraMode(SceneViewCameraMode value)
        {
            VirtualRoom.Handle.SetSceneViewCameraMode(value);
        }

        public void SetManipulationMode(ManipulationMode value)
        {
            SceneGraph.Handle.SetManipulationMode(value);
        }

        public void SetReferenceMode(ReferenceMode value)
        {
            SceneGraph.Handle.SetReferenceMode(value);
        }

        public void ToggleGizmo(string key, bool enabled)
        {
            ConfigurationSettings.Handle.SetValueBool(deviceName, key, enabled);
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && ConfigurationSettings.IsHandleConnected &&
                   VirtualRoom.IsHandleConnected && SceneGraph.IsHandleConnected;
        }

        private void FindModifiedGizmos()
        {
            modifiedGizmos.Clear();
            foreach (string gizmoKey in gizmoSettings.Keys)
            {
                string result;
                try {
                    result = ConfigurationSettings.Handle.GetValueJson(deviceName, gizmoKey);
                } 
                catch (ResultIsErrorException e) 
                {
                    Debug.LogWarning(e.Message);
                    continue;
                }

                if (bool.TryParse(result, out bool gizmoValue))
                {
                    MarkGizmoAsModified(gizmoKey, gizmoValue);
                }
                else
                {
                    Debug.LogError("Can't parse bool");
                }
            }
        }

        private SceneGizmo GetGizmo(string key)
        {
            string valueJson = ConfigurationSettings.Handle.GetPropertiesJson(deviceName, key);
            var gizmo = JsonUtility.FromJson<SceneGizmo>(valueJson);
            gizmo.Value = bool.Parse(ConfigurationSettings.Handle.GetValueJson(deviceName, key));
            return gizmo;
        }

        private void MarkGizmoAsModified(string key, bool value)
        {
            if (gizmoSettings.TryGetValue(key, out SceneGizmo gizmo))
            {
                if (gizmo.Value != value)
                {
                    gizmo.Value = value;
                    modifiedGizmos.Add(key, gizmo);
                }
            }
        }

        private void OnConfigurationSettingsChanged(ConfigurationSettingsChanges changes)
        {
            if (ZIBridge.IsDeviceMode)
                return;

            if (changes.HasFlag(ConfigurationSettingsChanges.ConfigurationSettingsChanged))
            {
                FindModifiedGizmos();
                SetModifiedGizmos();
            }
        }

        private void OnSceneGraphChanged(SceneGraphChanges sceneGraphChanges)
        {
            if (sceneGraphChanges.HasFlag(SceneGraphChanges.ReferenceModeChanged))
            {
                OnReferenceModeChanged?.Invoke(SceneGraph.Handle.GetReferenceMode());
            }

            if (sceneGraphChanges.HasFlag(SceneGraphChanges.ManipulationModeChanged))
            {
                OnManipulationModeChanged?.Invoke(SceneGraph.Handle.GetManipulationMode());
            }

            if (sceneGraphChanges.HasFlag(SceneGraphChanges.AnchorModeChanged))
            {
                OnAnchorModeChanged?.Invoke(SceneGraph.Handle.GetAnchorMode());
            }
        }

        private void OnVirtualRoomChanged(VirtualRoomChanges virtualRoomChanges)
        {
            if (virtualRoomChanges.HasFlag(VirtualRoomChanges.SceneViewCameraModeChanged))
            {
                OnCameraModeChanged?.Invoke(VirtualRoom.Handle.GetSceneViewCameraMode());
            }
        }

        private void SetGizmoValueInDictionary(string key, bool value)
        {
            if (gizmoSettings.TryGetValue(key, out SceneGizmo gizmo))
            {
                gizmo.Value = value;
                gizmoSettings[key] = gizmo;
                OnGizmoFlagChanged(key, gizmo.Value);
            }
        }

        private void SetModifiedGizmos()
        {
            foreach (KeyValuePair<string, SceneGizmo> pair in modifiedGizmos)
            {
                SceneGizmo gizmo = pair.Value;
                SetGizmoValueInDictionary(pair.Key, gizmo.Value);
            }
        }
    }
}
