// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System;
using System.IO;
using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal sealed partial class ZIBridge
    {
        private static readonly AsyncTask<bool> defaultSessionLoadingTask = new("Loading session.", "Magic Leap App Simulator",
            TaskDisplaySettings.ProgressBackground);
        private static readonly ProgressMonitor sceneGraphGetSceneGraphMonitor = ProgressMonitorDisplay.Show("Loading scene graph content.", "Magic Leap App Simulator");
        
        public readonly SceneGraphModule SceneGraph = new();

        public class SceneGraphModule : ModuleWrapper<SceneGraph, SceneGraphChanges>
        {
            public override void UpdateStatus()
            {
                var previousState = isConnected;
                base.UpdateStatus();

                if (previousState == IsConnected)
                    return;
                
                // When SceneGraph connect to session for the first time, load room and scene content.
                if (IsConnected)
                {
                    Instance.EnsureAnySceneContentLoaded();
                }
            }
        }

        private void EnsureAnySceneContentLoaded()
        {
            try
            {
                string defaultSessionPath = Path.IsPathFullyQualified(Settings.Instance.DefaultSessionPath) ?
                    Settings.Instance.DefaultSessionPath :
                    Path.Join(BackendPath, Settings.Instance.DefaultSessionPath);
                
                if (IsAnySceneContentLoaded() || string.IsNullOrEmpty(defaultSessionPath))
                {
                    return;
                }

                if (!File.Exists(defaultSessionPath))
                {
                    Debug.LogError($"Default session path does not exist ({defaultSessionPath}).\nYou can set it here: Project Settings/MagicLeap/App Simulator/General/Default Session");
                    return;
                }

                bool isRoomPath = Path.GetExtension(defaultSessionPath) == ".room";
                
                if (isRoomPath)
                {
                    defaultSessionLoadingTask.StartMonitored(monitor => SceneGraph.Handle.AddRoom(defaultSessionPath, monitor));
                }
                else
                {
                    LoadSession(defaultSessionPath);
                }
            }
            catch (ResultIsErrorException e)
            {
                Debug.LogException(e);
            }

            //Maybe we should have method exposed in sceneGraph to check this.
            bool IsAnySceneContentLoaded()
            {
                SceneNodeList list = SceneNodeList.Alloc();
                SceneGraph.Handle.GetSceneGraph(SceneGraphType.Room, list, sceneGraphGetSceneGraphMonitor);

                bool isLoaded = list.GetSize() > 0;
                list.Dispose();

                return isLoaded;
            }
        }
    }
}
