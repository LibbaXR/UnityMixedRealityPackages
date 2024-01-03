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
using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class ControllerViewModel : ViewModel
    {
        public event Action<bool> BumperChanged;
        public event Action<InputControllerType> ControllerTypeChanged;
        public event Action<bool> ControllerTypeEnabledChanged;
        public event Action<bool> FollowHeadPoseChanged;
        public event Action<float> GestureAngleChanged;
        public event Action<InputControllerTouchpadGestureDirection> GestureDirectionChanged;
        public event Action<float> GestureDistanceChanged;
        public event Action<Vector3> GesturePositionAndForceChanged;
        public event Action<float> GestureRadiusChanged;
        public event Action<float> GestureSpeedChanged;
        public event Action<InputControllerTouchpadGestureState> GestureStateChanged;
        public event Action<InputControllerTouchpadGestureType> GestureTypeChanged;

        public event Action<bool> ActiveOnDeviceChanged;
        public event Action<bool> InputConnectedChanged;
        public event Action<bool> MenuButtonChanged;
        public event Action<Quaternion> OrientationChanged;
        public event Action<Vector3> PositionChanged;
        public event Action<InputControllerSnapToHandposeMode> SnapToHandPoseChanged;
        public event Action<Vector3> TouchpadPositionAndForceChanged;
        public event Action<float> TriggerValueChanged;
        private ZIBridge.PausingModuleWrapper<InputController, InputControllerChanges> Input => Bridge.InputController;
        
        public override void Initialize()
        {
            Input.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            
            base.Initialize();
            
            Input.OnTakeChanges += OnControllerDataUpdated;
            Input.StartListening(this);
        }

        public override void UnInitialize()
        {
            Input.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            
            base.UnInitialize();
            Input.OnTakeChanges -= OnControllerDataUpdated;
            
            Input.StopListening(this);
        }
        
        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && Input.IsHandleConnected;
        }

        public bool GetActiveOnDevice()
        {
            return Input.Handle.GetActiveOnDevice();
        }

        public bool GetActionButtonState(InputControllerButton button)
        {
            return Input.Handle.GetButtonPressed(button);
        }

        public bool GetConnected()
        {
            return Input.Handle.GetConnected();
        }

        public InputControllerType GetControlType()
        {
            return Input.Handle.GetControllerType();
        }

        public bool GetFollowHeadpose()
        {
            return Input.Handle.GetFollowHeadpose();
        }

        public float GetGestureAngle()
        {
            return Input.Handle.GetTouchpadGestureAngle();
        }

        public InputControllerTouchpadGestureDirection GetGestureDirection()
        {
            return Input.Handle.GetTouchpadGestureDirection();
        }

        public float GetGestureDistance()
        {
            return Input.Handle.GetTouchpadGestureDistance();
        }

        public float GetGestureRadius()
        {
            return Input.Handle.GetTouchpadGestureRadius();
        }

        public float GetGestureSpeed()
        {
            return Input.Handle.GetTouchpadGestureSpeed();
        }

        public InputControllerTouchpadGestureState GetGestureState()
        {
            return Input.Handle.GetTouchpadGestureState();
        }

        public InputControllerTouchpadGestureType GetGestureType()
        {
            return Input.Handle.GetTouchpadGestureType();
        }

        public Vector3 GetInputGesturePositionAndForce()
        {
            return Input.Handle.GetTouchpadGesturePosAndForce().ToVec3();
        }

        public Vector3 GetInputTouchpadPosAndForce()
        {
            return Input.Handle.GetTouchPosAndForce(0).ToVec3();
        }

        public Quaternion GetOrientation()
        {
            return Input.Handle.GetOrientation().ToQuat();
        }

        public Vector3 GetPosition()
        {
            return Input.Handle.GetPosition().ToVec3();
        }

        public InputControllerSnapToHandposeMode GetSnapToHandPose()
        {
            return Input.Handle.GetSnapToHandposeMode();
        }

        public float GetTriggerValue()
        {
            return Input.Handle.GetTrigger();
        }

        public void SetActionButtonState(InputControllerButton button, bool state)
        {
            Input.Handle.SetButtonPressed(button, state);
        }

        public void SetConnected(bool isConnected)
        {
            Input.Handle.SetConnected(isConnected);
        }

        public void ResetTransform()
        {
            Input.Handle.ResetController();
        }

        public void SetControlType(InputControllerType value)
        {
            Input.Handle.SetControllerType(value);
        }

        public void SetFollowHeadpose(bool value)
        {
            Input.Handle.SetFollowHeadpose(value);
        }

        public void SetGestureAngle(float value)
        {
            Input.Handle.SetTouchpadGestureAngle(value);
        }

        public void SetGestureDirection(InputControllerTouchpadGestureDirection value)
        {
            Input.Handle.SetTouchpadGestureDirection(value);
        }

        public void SetGestureDistance(float value)
        {
            Input.Handle.SetTouchpadGestureDistance(value);
        }

        public void SetGestureRadius(float value)
        {
            Input.Handle.SetTouchpadGestureRadius(value);
        }

        public void SetGestureSpeed(float value)
        {
            Input.Handle.SetTouchpadGestureSpeed(value);
        }

        public void SetGestureState(InputControllerTouchpadGestureState value)
        {
            Input.Handle.SetTouchpadGestureState(value);
        }

        public void SetGestureType(InputControllerTouchpadGestureType value)
        {
            Input.Handle.SetTouchpadGestureType(value);
        }

        public void SetInputGesturePositionAndForce(Vector3 value)
        {
            Input.Handle.SetTouchpadGesturePosAndForce(value.ToVec3f());
        }

        public void SetInputTouchpadPosAndForce(Vector3 value)
        {
            Input.Handle.SetTouchPosAndForce(0, value.ToVec3f());
        }

        public void SetOrientation(Vector3 value)
        {
            Input.Handle.SetOrientation(Quaternion.Euler(value).ToQuatf(), true);
        }

        public void SetPosition(Vector3 value)
        {
            Input.Handle.SetPosition(value.ToVec3f(), true);
        }

        public void SetSnapToHandPose(InputControllerSnapToHandposeMode value)
        {
            Input.Handle.SetSnapToHandposeMode(value);
        }

        public void SetTouchpadActive(bool value)
        {
            Input.Handle.SetTouchActive(0, value);
        }

        public void SetTriggerValue(float value)
        {
            Input.Handle.SetTrigger(value);
        }

        private void OnControllerDataUpdated(InputControllerChanges changes)
        {
            if (changes.HasFlag(InputControllerChanges.ActiveOnDevice))
            {
                ActiveOnDeviceChanged?.Invoke(GetActiveOnDevice());
            }
            if (changes.HasFlag(InputControllerChanges.Connected))
            {
                InputConnectedChanged?.Invoke(GetConnected());
                ControllerTypeEnabledChanged?.Invoke(GetConnected());
            }

            if (changes.HasFlag(InputControllerChanges.Type))
            {
                ControllerTypeChanged?.Invoke(GetControlType());
            }

            if (changes.HasFlag(InputControllerChanges.Position))
            {
                PositionChanged?.Invoke(GetPosition());
            }

            if (changes.HasFlag(InputControllerChanges.Orientation))
            {
                OrientationChanged?.Invoke(GetOrientation());
            }

            if (changes.HasFlag(InputControllerChanges.Trigger))
            {
                TriggerValueChanged?.Invoke(GetTriggerValue());
            }

            if (changes.HasFlag(InputControllerChanges.Button))
            {
                BumperChanged?.Invoke(GetActionButtonState(InputControllerButton.Bumper));
                MenuButtonChanged?.Invoke(GetActionButtonState(InputControllerButton.Menu));
            }

            if (changes.HasFlag(InputControllerChanges.Touch))
            {
                TouchpadPositionAndForceChanged?.Invoke(GetInputTouchpadPosAndForce());
            }

            if (changes.HasFlag(InputControllerChanges.TouchpadGesture))
            {
                GesturePositionAndForceChanged?.Invoke(GetInputGesturePositionAndForce());
                GestureSpeedChanged?.Invoke(GetGestureSpeed());
                GestureDistanceChanged?.Invoke(GetGestureDistance());
                GestureRadiusChanged?.Invoke(GetGestureRadius());
                GestureAngleChanged?.Invoke(GetGestureAngle());
                GestureStateChanged?.Invoke(GetGestureState());
                GestureTypeChanged?.Invoke(GetGestureType());
                GestureDirectionChanged?.Invoke(GetGestureDirection());
            }

            if (changes.HasFlag(InputControllerChanges.FollowHeadpose))
            {
                FollowHeadPoseChanged?.Invoke(GetFollowHeadpose());
            }

            if (changes.HasFlag(InputControllerChanges.SnapToHandposeMode))
            {
                SnapToHandPoseChanged?.Invoke(GetSnapToHandPose());
            }
        }
    }
}
