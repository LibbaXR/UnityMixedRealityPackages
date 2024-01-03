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
    internal partial class HandTrackingViewModel : ViewModel
    {
        public readonly HandTrackingHandModel LeftHand = new(Hand.Left);
        public readonly HandTrackingHandModel RightHand = new(Hand.Right);

        public override void Initialize()
        {
            LeftHand.HandTracking.OnHandleConnectionChanged += SessionConnectionStatusChanged;
            RightHand.HandTracking.OnHandleConnectionChanged += SessionConnectionStatusChanged;

            base.Initialize();
            
            Bridge.LeftHandHandle.OnTakeChanges += LeftHand.HandDataChanged;
            Bridge.RightHandHandle.OnTakeChanges += RightHand.HandDataChanged;
            
            ZIBridge.Instance.LeftHandHandle.StartListening(this); 
            ZIBridge.Instance.RightHandHandle.StartListening(this); 
        }

        public override void UnInitialize()
        {
            LeftHand.HandTracking.OnHandleConnectionChanged -= SessionConnectionStatusChanged;
            RightHand.HandTracking.OnHandleConnectionChanged -= SessionConnectionStatusChanged;

            base.UnInitialize();

            Bridge.LeftHandHandle.OnTakeChanges -= LeftHand.HandDataChanged;
            Bridge.RightHandHandle.OnTakeChanges -= RightHand.HandDataChanged;

            ZIBridge.Instance.LeftHandHandle.StopListening(this);
            ZIBridge.Instance.RightHandHandle.StopListening(this);
        }

        protected override bool AreRequiredModulesConnected()
        {
            return ZIBridge.IsHandleConnected &&
                   ZIBridge.Instance.LeftHandHandle.IsHandleConnected &&
                   ZIBridge.Instance.RightHandHandle.IsHandleConnected;
        }
    }
}
