// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal class SceneCameraViewModel : AlignableTransformViewModel
    {
        private ZIBridge.ModuleWrapper<VirtualRoom, VirtualRoomChanges> VirtualRoom => ZIBridge.Instance.VirtualRoom;
        
        public override void Initialize()
        {
            VirtualRoom.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            base.Initialize();
            Bridge.VirtualRoom.OnTakeChanges += VirtualRoomChanged;
        }

        public override void UnInitialize()
        {
            VirtualRoom.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            base.UnInitialize();
            Bridge.VirtualRoom.OnTakeChanges -= VirtualRoomChanged;
        }

        private void VirtualRoomChanged(VirtualRoomChanges changes)
        {
            if (changes.HasFlag(VirtualRoomChanges.SceneViewCameraOrientationChanged)) OrientationChanged();

            if (changes.HasFlag(VirtualRoomChanges.SceneViewCameraPositionChanged)) PositionChanged();
        }

        public override Quaternion GetOrientation()
        {
            return VirtualRoom.Handle.GetSceneViewCameraOrientation().ToQuat();
        }

        public override void SetPosition(Vector3 newPosition)
        {
            VirtualRoom.Handle.SetSceneViewCameraPosition(newPosition.ToVec3f(), true);
        }

        public override void SetOrientation(Quaternion quaternion)
        {
            VirtualRoom.Handle.SetSceneViewCameraOrientation(quaternion.ToQuatf(), true);
        }

        public override Vector3 GetPosition()
        {
            return VirtualRoom.Handle.GetSceneViewCameraPosition().ToVec3();
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected && ZIBridge.Instance.VirtualRoom.IsHandleConnected &&
                   ZIBridge.Instance.HeadTrackingHandle.IsHandleConnected;
        }

        public void ResetSceneViewCamera()
        {
            VirtualRoom.Handle.ResetSceneViewCamera();
        }
    }
}
