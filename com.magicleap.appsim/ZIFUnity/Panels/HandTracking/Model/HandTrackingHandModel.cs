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
    internal partial class HandTrackingViewModel
    {
        internal class HandTrackingHandModel
        {
            public event Action<float> ConfidenceChanged;
            public event Action<bool> FollowHeadPoseChanged;
            public event Action<bool> IsHoldingControlChanged;
            public event Action<Vector3> OrientationChanged;
            public event Action<Vector3> PositionChanged;
            public event Action<HandTrackingGesture> TrackingKeyPoseChanged;
            public event Action<bool> ActiveOnDeviceChanged;

            private readonly Hand hand;

            internal ZIBridge.ModuleWrapper<HandTracking, HandTrackingChanges> HandTracking =>
                hand == Hand.Left ? ZIBridge.Instance.LeftHandHandle : ZIBridge.Instance.RightHandHandle;
                

            public HandTrackingHandModel(Hand hand)
            {
                this.hand = hand;
            }

            public float GetConfidence() => HandTracking.Handle.GetHandConfidence();
            public bool GetHoldingControl() => HandTracking.Handle.GetHoldingControl();
            public Vector3 GetOrientation() => HandTracking.Handle.GetOrientation().ToQuat().eulerAngles;
            public Vector3 GetPosition() => HandTracking.Handle.GetPosition().ToVec3();
            public HandTrackingGesture GetTrackingGesture() => HandTracking.Handle.GetGesture();
            public bool GetFollowHeadPose() => HandTracking.Handle.GetFollowHeadpose();
            public bool GetActiveOnDevice() => HandTracking.Handle.GetActiveOnDevice();

            public void HandDataChanged(HandTrackingChanges changes)
            {
                if (changes.HasFlag(HandTrackingChanges.ActiveOnDevice))
                {
                    ActiveOnDeviceChanged?.Invoke(GetActiveOnDevice());
                }

                if (changes.HasFlag(HandTrackingChanges.Gesture))
                {
                    TrackingKeyPoseChanged?.Invoke(GetTrackingGesture());
                }

                if (changes.HasFlag(HandTrackingChanges.HandConfidence))
                {
                    ConfidenceChanged?.Invoke(GetConfidence());
                }

                if (changes.HasFlag(HandTrackingChanges.HoldingControl))
                {
                    IsHoldingControlChanged?.Invoke(GetHoldingControl());
                }

                if (changes.HasFlag(HandTrackingChanges.Position))
                {
                    PositionChanged?.Invoke(GetPosition());
                }

                if (changes.HasFlag(HandTrackingChanges.Orientation))
                {
                    OrientationChanged?.Invoke(GetOrientation());
                }

                if (changes.HasFlag(HandTrackingChanges.FollowHeadpose))
                {
                    FollowHeadPoseChanged?.Invoke(GetFollowHeadPose());
                }
            }

            public void SetConfidence(float value) => HandTracking.Handle.SetHandConfidence(value);
            public void SetHoldingControl(bool value) => HandTracking.Handle.SetHoldingControl(value);
            public void SetOrientation(Vector3 value) => HandTracking.Handle.SetOrientation(Quaternion.Euler(value).ToQuatf(), true);
            public void SetPosition(Vector3 value) => HandTracking.Handle.SetPosition(value.ToVec3f(), true);
            public void SetTrackingKeyPose(HandTrackingGesture value) => HandTracking.Handle.SetGesture(value);
            public void SetFollowHeadPose(bool value) => HandTracking.Handle.SetFollowHeadpose(value);

            public void ResetHandCenter() => HandTracking.Handle.ResetHandGizmoLocation();
        }
    }
}
