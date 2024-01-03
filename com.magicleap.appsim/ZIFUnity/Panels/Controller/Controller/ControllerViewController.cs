// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Runtime.CompilerServices;
using ml.zi;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

namespace MagicLeap.ZI
{
    internal class ControllerViewController : ViewController<ControllerViewModel, ControllerViewPresenter>
    {
        private static readonly string windowName = "App Sim Controller";

#if !UNITY_EDITOR_OSX
        [MenuItem("Window/Magic Leap App Simulator/App Sim Controller #F6", false, MenuItemPriority_Controller)]
#else
        [MenuItem("Window/Magic Leap App Simulator/App Sim Controller", isValidateFunction: false, priority: MenuItemPriority_Controller)]
#endif
        public static void ShowWindow()
        {
            GetWindow<ControllerViewController>(windowName);
        }

        private void OnDisable()
        {
            RemoveModelListeners();

            RemoveViewListeners();
            Presenter.OnDisable();
            Model.UnInitialize();
        }

        protected override void Initialize()
        {
            AddModelListeners();

            Presenter.OnEnable(rootVisualElement);
            AddViewListeners();

            base.Initialize();
        }

        private void AddModelListeners()
        {
            Model.OnSessionStarted += OnSessionStarted;
            Model.OnSessionStopped += Presenter.DisableAllControls;

            Model.FollowHeadPoseChanged += Presenter.SetFollowHeadPose;
            Model.PositionChanged += Presenter.SetControllerPosition;
            Model.OrientationChanged += Presenter.SetControllerOrientation;
            Model.TouchpadPositionAndForceChanged += Presenter.SetTouchpadPositionAndForce;
            Model.TriggerValueChanged += Presenter.SetTriggerValue;
            Model.BumperChanged += Presenter.SetBumperButtonState;
            Model.MenuButtonChanged += Presenter.SetMenuButtonState;

            Model.GesturePositionAndForceChanged += Presenter.SetGesturePositionAndForce;
            Model.GestureSpeedChanged += Presenter.SetGestureSpeed;
            Model.GestureDistanceChanged += Presenter.SetGestureDistance;
            Model.GestureRadiusChanged += Presenter.SetGestureRadius;
            Model.GestureAngleChanged += Presenter.SetGestureAngle;
            Model.GestureStateChanged += Presenter.SetGestureState;
            Model.GestureTypeChanged += Presenter.SetGestureType;
            Model.GestureDirectionChanged += Presenter.SetGestureDirection;

            Model.InputConnectedChanged += Presenter.SetInputConnection;
            Model.ControllerTypeEnabledChanged += OnControllerTypeChanged;
            Model.ControllerTypeChanged += Presenter.SetControllerType;
            Model.SnapToHandPoseChanged += SetSnapToHandPose;
            Model.ActiveOnDeviceChanged += SetActiveOnDevice;
        }
        
        private void AddViewListeners()
        {
            Presenter.OnResetTransformClicked += Model.ResetTransform;
            Presenter.SnapToHandPoseChanged += Model.SetSnapToHandPose;
            Presenter.ControllerPositionChanged += Model.SetPosition;
            Presenter.ControllerOrientationChanged += Model.SetOrientation;
            Presenter.ControllerFollowHeadPoseChanged += Model.SetFollowHeadpose;

            Presenter.TouchpadPositionAndForceChanged += Model.SetInputTouchpadPosAndForce;
            Presenter.TouchpadStateChanged += Model.SetTouchpadActive;
            Presenter.TriggerValueChanged += Model.SetTriggerValue;
            Presenter.ActionButtonStateChanged += Model.SetActionButtonState;

            Presenter.GesturePositionChanged += Model.SetInputGesturePositionAndForce;
            Presenter.GestureSpeedChanged += Model.SetGestureSpeed;
            Presenter.GestureDistanceChanged += Model.SetGestureDistance;
            Presenter.GestureRadiusChanged += Model.SetGestureRadius;
            Presenter.GestureAngleChanged += Model.SetGestureAngle;

            Presenter.GestureTypeChanged += Model.SetGestureType;
            Presenter.GestureStateChanged += Model.SetGestureState;
            Presenter.GestureDirectionChanged += Model.SetGestureDirection;

            Presenter.InputConnectionChanged += Model.SetConnected;
            Presenter.ControllerTypeChanged += Model.SetControlType;
        }

        private void OnControllerTypeChanged(bool enabled)
        {
            Presenter.SetControllerTypeEnabled(!enabled);
        }

        private void OnSessionStarted()
        {
            Presenter.SetInteractable(true);
            SetActiveOnDevice(Model.GetActiveOnDevice());

            Presenter.DisableDeviceFields(Model.IsDeviceMode);
            
            Presenter.SetFollowHeadPose(Model.GetFollowHeadpose());
            Presenter.SetControllerPosition(Model.GetPosition());
            Presenter.SetControllerOrientation(Model.GetOrientation());
            Presenter.SetTouchpadPositionAndForce(Model.GetInputTouchpadPosAndForce());
            Presenter.SetTriggerValue(Model.GetTriggerValue());
            Presenter.SetBumperButtonState(Model.GetActionButtonState(InputControllerButton.Bumper));
            Presenter.SetMenuButtonState(Model.GetActionButtonState(InputControllerButton.Menu));

            Presenter.SetGesturePositionAndForce(Model.GetInputGesturePositionAndForce());
            Presenter.SetGestureSpeed(Model.GetGestureSpeed());
            Presenter.SetGestureDistance(Model.GetGestureDistance());
            Presenter.SetGestureRadius(Model.GetGestureRadius());
            Presenter.SetGestureAngle(Model.GetGestureAngle());
            Presenter.SetGestureState(Model.GetGestureState());
            Presenter.SetGestureType(Model.GetGestureType());
            Presenter.SetGestureDirection(Model.GetGestureDirection());
            Presenter.SetControllerType(Model.GetControlType());
            Presenter.SetInputConnection(Model.GetConnected());
            
            SetSnapToHandPose(Model.GetSnapToHandPose());
        }

        private void RemoveModelListeners()
        {
            Model.OnSessionStarted -= OnSessionStarted;
            Model.OnSessionStopped -= Presenter.DisableAllControls;

            Model.FollowHeadPoseChanged -= Presenter.SetFollowHeadPose;
            Model.PositionChanged -= Presenter.SetControllerPosition;
            Model.OrientationChanged -= Presenter.SetControllerOrientation;
            Model.TouchpadPositionAndForceChanged -= Presenter.SetTouchpadPositionAndForce;
            Model.TriggerValueChanged -= Presenter.SetTriggerValue;
            Model.BumperChanged -= Presenter.SetBumperButtonState;
            Model.MenuButtonChanged -= Presenter.SetMenuButtonState;

            Model.GesturePositionAndForceChanged -= Presenter.SetGesturePositionAndForce;
            Model.GestureSpeedChanged -= Presenter.SetGestureSpeed;
            Model.GestureDistanceChanged -= Presenter.SetGestureDistance;
            Model.GestureRadiusChanged -= Presenter.SetGestureRadius;
            Model.GestureAngleChanged -= Presenter.SetGestureAngle;
            Model.GestureStateChanged -= Presenter.SetGestureState;
            Model.GestureTypeChanged -= Presenter.SetGestureType;
            Model.GestureDirectionChanged -= Presenter.SetGestureDirection;

            Model.InputConnectedChanged -= Presenter.SetInputConnection;
            Model.ControllerTypeEnabledChanged -= OnControllerTypeChanged;
            Model.ControllerTypeChanged -= Presenter.SetControllerType;
            Model.SnapToHandPoseChanged -= SetSnapToHandPose;
            Model.ActiveOnDeviceChanged -= SetActiveOnDevice;
        }

        private void RemoveViewListeners()
        {
            Presenter.OnResetTransformClicked -= Model.ResetTransform;
            Presenter.SnapToHandPoseChanged -= Model.SetSnapToHandPose;
            Presenter.ControllerPositionChanged -= Model.SetPosition;
            Presenter.ControllerOrientationChanged -= Model.SetOrientation;
            Presenter.ControllerFollowHeadPoseChanged -= Model.SetFollowHeadpose;

            Presenter.TouchpadPositionAndForceChanged -= Model.SetInputTouchpadPosAndForce;
            Presenter.TouchpadStateChanged -= Model.SetTouchpadActive;
            Presenter.TriggerValueChanged -= Model.SetTriggerValue;
            Presenter.ActionButtonStateChanged -= Model.SetActionButtonState;

            Presenter.GesturePositionChanged -= Model.SetInputGesturePositionAndForce;
            Presenter.GestureSpeedChanged -= Model.SetGestureSpeed;
            Presenter.GestureDistanceChanged -= Model.SetGestureDistance;
            Presenter.GestureRadiusChanged -= Model.SetGestureRadius;
            Presenter.GestureAngleChanged -= Model.SetGestureAngle;

            Presenter.GestureTypeChanged -= Model.SetGestureType;
            Presenter.GestureStateChanged -= Model.SetGestureState;
            Presenter.GestureDirectionChanged -= Model.SetGestureDirection;

            Presenter.InputConnectionChanged -= Model.SetConnected;
            Presenter.ControllerTypeChanged -= Model.SetControlType;
        }

        private void SetActiveOnDevice(bool activeOnDevice)
        {
            SetControllerEnabled(Model.GetSnapToHandPose(), Model.GetActiveOnDevice());
        }

        private void SetSnapToHandPose(InputControllerSnapToHandposeMode snapMode)
        {
            if (Model.IsDeviceMode)
                return;
            
            Presenter.SetSnapToHandPose(snapMode);
            SetControllerEnabled(snapMode, Model.GetActiveOnDevice());
        }
        
        private void SetControllerEnabled(InputControllerSnapToHandposeMode snapMode, bool activeOnDevice)
        {
            bool isEnabled = snapMode == InputControllerSnapToHandposeMode.Disabled && !activeOnDevice;
            Presenter.SetControllerEnabled(isEnabled);
            Presenter.SetSnapToHandPoseEnabled(!activeOnDevice);
        }
    }
}
