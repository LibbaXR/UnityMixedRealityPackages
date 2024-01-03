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
    internal class ControllerViewState : IViewState
    {
        public bool IsTransformOpen;
        public bool IsTouchpadOpen;
        public bool IsActionsOpen;
        public bool IsGesturesOpen;

        public bool IsActionHoldToggled;
        public float TouchPadForce;

        public bool IsTriggerButtonHeld;
        public bool IsBumperButtonHeld;
        public bool IsMenuButtonHeld;
        public bool IsBackButtonHeld;

        public void SetDefaultValues()
        {
            TouchPadForce = 0.3F;
            IsTransformOpen = true;
            IsTouchpadOpen = true;
            IsActionsOpen = true;
            IsGesturesOpen = true;

            IsActionHoldToggled = false;
        }
    }
}
