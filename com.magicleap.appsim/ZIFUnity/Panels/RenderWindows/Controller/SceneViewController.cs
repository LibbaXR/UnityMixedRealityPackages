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
using System.Threading.Tasks;
using ml.zi;
using UnityEditor;

namespace MagicLeap.ZI
{
    internal class SceneViewController : RenderViewController<SceneRenderViewModel, SceneRenderViewPresenter, SceneRenderViewState>
    {
        private static readonly string windowName = "App Sim Scene View";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Scene View #F3", false, MenuItemPriority_SceneView)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Scene View", isValidateFunction: false, priority: MenuItemPriority_SceneView)]
#endif
        private static void ShowWindow()
        {
            GetWindow<SceneViewController>(windowName);
        }

        protected override void OnDisable()
        {
            UnregisterPresenterCallbacks();
            UnregisterModelCallbacks();

            base.OnDisable();
            
            Model.UnInitialize();
        }

        protected override void Initialize()
        {
            RegisterPresenterCallbacks();
            RegisterModelCallbacks();

            base.Initialize();
        }

        private void HandleAddModelButtonClicked()
        {
            string path = EditorUtility.OpenFilePanel("Add Model", Model.PreviousModelDirectory ?? "", "fbx,obj,gltf");
            Task.Run(() => Model.Add3DModel(path));
        }

        private void HandleAddRoomButtonClicked()
        {
            string path = EditorUtility.OpenFilePanel("Add Room", Model.PreviousRoomDirectory ?? "", "room");
            Task.Run(() => Model.AddRoom(path));
        }

        private void OnSessionStarted()
        {
            Presenter.SelectAnchorModeWithoutNotify(Model.GetAnchorMode());
            Presenter.SelectCameraModeWithoutNotify(Model.GetCameraMode());
            Presenter.SelectManipulationModeWithoutNotify(Model.GetManipulationMode());
            Presenter.SelectReferenceModeWithoutNotify(Model.GetReferenceMode());

            SetGizmos(Model.GetAllGizmos());
        }

        private void PresenterOnAnchorModeSelected(int anchorMode)
        {
            Model.SetAnchorMode((AnchorMode) anchorMode);
        }

        private void PresenterOnCameraModeSelected(int cameraMode)
        {
            Model.SetCameraMode((SceneViewCameraMode) cameraMode);
        }

        private void PresenterOnManipulationModeSelected(int manipulationMode)
        {
            Model.SetManipulationMode((ManipulationMode) manipulationMode);
        }

        private void PresenterOnReferenceModeSelected(int referenceMode)
        {
            Model.SetReferenceMode((ReferenceMode) referenceMode);
        }

        private void RegisterModelCallbacks()
        {
            Model.OnSessionStarted += OnSessionStarted;

            Model.OnGizmoFlagChanged += Presenter.SetGizmoToggleValue;
            Model.OnManipulationModeChanged += Presenter.SelectManipulationModeWithoutNotify;
            Model.OnAnchorModeChanged += Presenter.SelectAnchorModeWithoutNotify;
            Model.OnReferenceModeChanged += Presenter.SelectReferenceModeWithoutNotify;
            Model.OnCameraModeChanged += Presenter.SelectCameraModeWithoutNotify;
        }

        private void RegisterPresenterCallbacks()
        {
            Presenter.AddRoomButtonHandler += HandleAddRoomButtonClicked;
            Presenter.AddModelButtonHandler += HandleAddModelButtonClicked;
            Presenter.ClearSceneButtonHandler += Model.ClearScene;
            Presenter.ToggleGizmoHandler += Model.ToggleGizmo;
            Presenter.ManipulationModeSelected += PresenterOnManipulationModeSelected;
            Presenter.AnchorModeSelected += PresenterOnAnchorModeSelected;
            Presenter.ReferenceModeSelected += PresenterOnReferenceModeSelected;
            Presenter.CameraModeSelected += PresenterOnCameraModeSelected;
            Presenter.ResetCameraSelected += Model.ResetCamera;
        }

        private void SetGizmos(IEnumerable<SceneGizmo> gizmoSettings)
        {
            Model.ClearGizmos();
            Presenter.ClearGizmos();
            foreach (SceneGizmo gizmo in gizmoSettings)
            {
                Model.AddGizmo(gizmo);
                Presenter.AddGizmo(gizmo);
            }
        }

        private void UnregisterModelCallbacks()
        {
            Model.OnSessionStarted -= OnSessionStarted;

            Model.OnGizmoFlagChanged -= Presenter.SetGizmoToggleValue;
            Model.OnManipulationModeChanged -= Presenter.SelectManipulationModeWithoutNotify;
            Model.OnAnchorModeChanged -= Presenter.SelectAnchorModeWithoutNotify;
            Model.OnReferenceModeChanged -= Presenter.SelectReferenceModeWithoutNotify;
            Model.OnCameraModeChanged -= Presenter.SelectCameraModeWithoutNotify;
        }

        private void UnregisterPresenterCallbacks()
        {
            Presenter.AddRoomButtonHandler -= HandleAddRoomButtonClicked;
            Presenter.AddModelButtonHandler -= HandleAddModelButtonClicked;
            Presenter.ClearSceneButtonHandler -= Model.ClearScene;
            Presenter.ToggleGizmoHandler -= Model.ToggleGizmo;
            Presenter.ManipulationModeSelected -= PresenterOnManipulationModeSelected;
            Presenter.AnchorModeSelected -= PresenterOnAnchorModeSelected;
            Presenter.ReferenceModeSelected -= PresenterOnReferenceModeSelected;
            Presenter.CameraModeSelected -= PresenterOnCameraModeSelected;
            Presenter.ResetCameraSelected -= Model.ResetCamera;
        }
    }
}
