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

namespace MagicLeap.ZI
{
    [Serializable]
    internal class TargetViewState : IViewState
    {
        public enum SelectableTargets
        {
            Simulator = SessionTargetMode.Simulator,
            Device = SessionTargetMode.Device,
            // Disable hybrid mode......2023.08.10
            // Hybrid = SessionTargetMode.Hybrid
        }

        public SelectableTargets TargetMode;

        public void SetDefaultValues()
        {
            TargetMode = SelectableTargets.Simulator;
        }
    }
}
