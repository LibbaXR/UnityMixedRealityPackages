// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Specialized;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class ImageTrackingViewController : ViewController<ImageTrackingViewModel, ImageTrackingViewPresenter>
    {
        private static readonly string windowName = "App Sim Marker Tracking";

        // Hidden for now.  Let any existing layouts persist, but don't show to users.
#if !UNITY_EDITOR_OSX
    [MenuItem("Window/Magic Leap App Simulator/App Sim Marker Tracking #F8", false, MenuItemPriority_MarkerTracking)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Marker Tracking", isValidateFunction: false, priority: MenuItemPriority_MarkerTracking)]
#endif
        public static void ShowWindow()
        {
            GetWindow<ImageTrackingViewController>(windowName);
        }

        private void OnDisable()
        {
            Presenter.NewImageTargetCreated -= OnNewImageTargetCreated;
            Presenter.FollowHeadPoseChanged -= OnFollowHeadPoseChanged;
            Presenter.ImageTargetCloned -= OnImageTargetCloned;
            Presenter.ImageTargetRemoved -= OnImageTargetRemoved;
            Presenter.OnDisable();

            Model.OnSessionStarted -= OnSessionStarted;
            Model.OnSessionStopped -= OnSessionStopped;
            Model.ImageTargets.CollectionChanged -= OnImageTargetsOnCollectionChanged;
            Model.ImageTargetModelUpdated -= OnImageTargetModelUpdated;
            Model.FollowHeadPoseUpdated -= OnFollowHeadPoseUpdated;
            Model.ActiveOnDeviceUpdated -= Presenter.ActiveOnDeviceChanged;
            EditorSceneManager.activeSceneChangedInEditMode -= OnSceneChangedInEditMode;
            Model.UnInitialize();
        }

        public override void AddItemsToMenu(GenericMenu menu)
        {
            base.AddItemsToMenu(menu);

            var content = new GUIContent("Clear all image targets");
            menu.AddItem(content, false, ClearAllImageTargets);
        }

        protected override void Initialize()
        {
            Presenter.NewImageTargetCreated += OnNewImageTargetCreated;
            Presenter.FollowHeadPoseChanged += OnFollowHeadPoseChanged;
            Presenter.ImageTargetCloned += OnImageTargetCloned;
            Presenter.ImageTargetRemoved += OnImageTargetRemoved;
            Presenter.OnEnable(rootVisualElement);

            Model.OnSessionStarted += OnSessionStarted;
            Model.OnSessionStopped += OnSessionStopped;
            Model.ImageTargets.CollectionChanged += OnImageTargetsOnCollectionChanged;
            Model.ImageTargetModelUpdated += OnImageTargetModelUpdated;
            Model.FollowHeadPoseUpdated += OnFollowHeadPoseUpdated;
            Model.ActiveOnDeviceUpdated += Presenter.ActiveOnDeviceChanged;
            base.Initialize();
            Presenter.SetEnabled( Model.IsSessionRunning && !Model.GetActiveOnDevice());

            EditorSceneManager.activeSceneChangedInEditMode += OnSceneChangedInEditMode;
        }

        private void ClearAllImageTargets()
        {
            Model.ClearAllTargets();
        }

        private void OnFollowHeadPoseChanged(bool followEnabled)
        {
            Model.SetFollowHeadPose(followEnabled);
        }

        private void OnFollowHeadPoseUpdated(bool enabled)
        {
            Presenter.OnFollowHeadPoseUpdated(enabled);
        }

        private void OnImageTargetModelUpdated(string nodeId)
        {
            Presenter.OnImageTargetModelUpdated(nodeId);
        }

        private void OnImageTargetCloned(string nodeId)
        {
            Model.CloneImageTarget(nodeId);
        }

        private void OnImageTargetRemoved(string nodeId)
        {
            Model.RemoveImageTarget(nodeId);
        }

        private void OnImageTargetsOnCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            Presenter.OnImageTargetsCollectionChanged(e);
        }

        private void OnNewImageTargetCreated(string markerType)
        {
            Model.CreateNewImageTarget(markerType);
        }

        private void OnSessionStarted()
        {
            Presenter.SetEnabled(!Model.GetActiveOnDevice());
            Presenter.OnFollowHeadPoseUpdated(Model.GetFollowHeadPose());
        }

        private void OnSessionStopped()
        {
            Presenter.SetEnabled(false);
        }

        private void OnSceneChangedInEditMode(UnityEngine.SceneManagement.Scene scene1, UnityEngine.SceneManagement.Scene scene2)
        {
            Model.RefreshMarkers();
        }
    }
}
