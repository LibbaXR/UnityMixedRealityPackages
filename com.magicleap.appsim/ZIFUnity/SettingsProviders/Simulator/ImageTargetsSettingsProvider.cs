// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) (2021-2023) Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using System.Collections.Generic;
using ml.zi;
using UnityEditor;

namespace MagicLeap.ZI
{
    internal class ImageTargetsSettingsProvider
    {
        [SettingsProvider]
        public static UnityEditor.SettingsProvider RenderSettingProvider()
        {
            return new ProjectSettingsProvider(
                "MagicLeap/App Simulator/Simulator/Image Targets",
                @"Simulator/Image Targets",
                "Image Targets",
                new List<SessionTargetMode> {SessionTargetMode.Simulator});
        }
    }
}
