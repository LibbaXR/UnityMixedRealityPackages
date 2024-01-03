// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2019-2022) Magic Leap, Inc. All Rights Reserved.
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
    internal class HeadPoseDriver : IPoseDriver
    {
        private readonly ZIBridge.PausingModuleWrapper<HeadTracking, HeadTrackingChanges> headTracking = ZIBridge.Instance.HeadTrackingHandle;

        public SceneMovementMode MovementMode => (SceneMovementMode) Settings.Instance.GameViewMovementMode;

        public void Initialize()
        {
            headTracking.StartListening(this);
        }

        public void Deinitialize()
        {
            headTracking.StopListening(this);
        }

        public float MovementSpeed => Settings.Instance.GameViewMoveSpeed;

        public float RotationSpeed => Settings.Instance.GameViewRotationSpeed;

        public void Rotate(Quaternion delta)
        {
            if (headTracking.IsHandleConnected)
            {
                var ziDelta = new Quaternionf();
                ziDelta.FromQuat(Utils.ToMLCoordinates(delta));
                CallAndLog(() => headTracking.Handle.SetOrientation(ziDelta, false));
            }
        }

        public void Translate(Vector3 delta)
        {
            if (headTracking.IsHandleConnected)
            {
                var ziDelta = new Vec3f();
                ziDelta.FromVec3(Utils.ToMLCoordinates(delta));
                CallAndLog(() => headTracking.Handle.SetPosition(ziDelta, false));
            }
        }

        private void CallAndLog(Action action)
        {
            try
            {
                action();
            }
            catch (ResultIsErrorException e)
            {
                Debug.Log(e.Message);
            }
        }
    }
}
