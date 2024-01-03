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
using UnityEngine;

namespace MagicLeap.ZI
{
    internal abstract class AlignableTransformViewModel : ViewModel
    {
        public event Action<Vector3> OnPositionChanged;
        public event Action<Quaternion> OnOrientationChanged;

        protected void PositionChanged()
        {
            OnPositionChanged?.Invoke(GetPosition());
        }

        protected void OrientationChanged()
        {
            OnOrientationChanged?.Invoke(GetOrientation());
        }

        public abstract Vector3 GetPosition();
        public abstract Quaternion GetOrientation();
        
        public void AlignDeviceToSceneView()
        {
            var cameraOrientation = Bridge.HeadTrackingHandle.Handle.GetOrientation();
            var cameraPosition = Bridge.HeadTrackingHandle.Handle.GetPosition();

            Bridge.VirtualRoom.Handle.SetSceneViewCameraPosition(cameraPosition, true);
            Bridge.VirtualRoom.Handle.SetSceneViewCameraOrientation(cameraOrientation, true);
        }

        public void AlignSceneViewToDevice()
        {
            var cameraOrientation = Bridge.VirtualRoom.Handle.GetSceneViewCameraOrientation();
            var cameraPosition = Bridge.VirtualRoom.Handle.GetSceneViewCameraPosition();

            Bridge.HeadTrackingHandle.Handle.SetPosition(cameraPosition, true);
            Bridge.HeadTrackingHandle.Handle.SetOrientation(cameraOrientation, true);
        }

        public abstract void SetPosition(Vector3 newPosition);
        public abstract void SetOrientation(Quaternion quaternion);
    }
}
