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
    internal class HeadPoseViewModel : AlignableTransformViewModel
    {
        public event Action<float> ConfidenceChanged;
        public event Action<HeadTrackingError> ErrorChanged;
        public event Action<HeadTrackingMode> HeadTrackingModeChanged;
        public event Action<HeadTrackingMapEvents> MapEventsChanged;

        private ZIBridge.ModuleWrapper<HeadTracking, HeadTrackingChanges> HeadTracking => ZIBridge.Instance.HeadTrackingHandle;

        public override void Initialize()
        {
            HeadTracking.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
            Bridge.HeadTrackingHandle.OnTakeChanges += HeadTrackingChanged;
            Bridge.HeadTrackingHandle.StartListening(this);
        }

        public override void UnInitialize()
        {
            HeadTracking.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
            Bridge.HeadTrackingHandle.OnTakeChanges-= HeadTrackingChanged;
            Bridge.HeadTrackingHandle.StopListening(this);
        }
        
        public void ResetHeadPose()
        {
            HeadTracking.Handle.ResetHeadpose();
        }

        public float GetConfidence()
        {
            return HeadTracking.Handle.GetConfidence();
        }

        public HeadTrackingError GetError()
        {
            return HeadTracking.Handle.GetError();
        }

        public HeadTrackingMode GetHeadTrackingMode()
        {
            return HeadTracking.Handle.GetMode();
        }

        public HeadTrackingMapEvents GetMapEvents()
        {
            return HeadTracking.Handle.TakeMapEvents();
        }
        

        public override Quaternion GetOrientation()
        {
            return HeadTracking.Handle.GetOrientation().ToQuat();
        }

        public override Vector3 GetPosition()
        {
            return HeadTracking.Handle.GetPosition().ToVec3();
        }

        public void SetConfidence(float confidence)
        {
            HeadTracking.Handle.SetConfidence(confidence);
        }

        public void SetError(HeadTrackingError error)
        {
            HeadTracking.Handle.SetError(error);
        }

        public void SetHeadTrackingMode(HeadTrackingMode mode)
        {
            HeadTracking.Handle.SetMode(mode);
        }

        public void SetMapEvents(HeadTrackingMapEvents events)
        {
            HeadTracking.Handle.PostMapEvents(events);
        }

        public override void SetOrientation(Quaternion rotation)
        {
            HeadTracking.Handle.SetOrientation(rotation.ToQuatf(), true);
        }

        public override void SetPosition(Vector3 position)
        {
            HeadTracking.Handle.SetPosition(position.ToVec3f(), true);
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected &&
                ZIBridge.Instance.HeadTrackingHandle.IsHandleConnected;
        }

        private void HeadTrackingChanged(HeadTrackingChanges changes)
        {
            if (changes.HasFlag(HeadTrackingChanges.MapEvents))
            {
                MapEventsChanged?.Invoke(GetMapEvents());
            }

            if (changes.HasFlag(HeadTrackingChanges.Position))
            {
                PositionChanged();
            }

            if (changes.HasFlag(HeadTrackingChanges.Orientation))
            {
                OrientationChanged();
            }

            if (changes.HasFlag(HeadTrackingChanges.Mode))
            {
                HeadTrackingModeChanged?.Invoke(GetHeadTrackingMode());
            }

            if (changes.HasFlag(HeadTrackingChanges.Confidence))
            {
                ConfidenceChanged?.Invoke(GetConfidence());
            }

            if (changes.HasFlag(HeadTrackingChanges.Error))
            {
                ErrorChanged?.Invoke(GetError());
            }
        }
    }
}
