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

namespace MagicLeap.ZI
{
    [Serializable]
    internal class HandTrackingViewState : IViewState
    {
        public bool LeftHoldPose;
        public bool RightHoldPose;

        public bool LeftHandFoldoutOpened = true;
        public bool LeftHandTransformFoldoutOpened = true;

        public bool RightHandFoldoutOpened = true;
        public bool RightHandTransformFoldoutOpened = true;

        public void SetDefaultValues()
        {
            LeftHoldPose = false;
            RightHoldPose = false;

            LeftHandFoldoutOpened = true;
            LeftHandTransformFoldoutOpened = true;
            RightHandFoldoutOpened = true;
            RightHandTransformFoldoutOpened = true;
        }

        public void SetLeftHandContentFoldout(bool isOpened)
        {
            LeftHandFoldoutOpened = isOpened;
        }

        public void SetLeftHandTransformFoldout(bool isOpened)
        {
            LeftHandTransformFoldoutOpened = isOpened;
        }

        public void SetRightHandContentFoldout(bool isOpened)
        {
            RightHandFoldoutOpened = isOpened;
        }

        public void SetRightHandTransformFoldout(bool isOpened)
        {
            RightHandTransformFoldoutOpened = isOpened;
        }
    }
}
