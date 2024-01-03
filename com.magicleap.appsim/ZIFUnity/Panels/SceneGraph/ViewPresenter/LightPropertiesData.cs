// %BANNER_BEGIN%
// ---------------------------------------------------------------------
// %COPYRIGHT_BEGIN%
// Copyright (c) 2022 Magic Leap, Inc. All Rights Reserved.
// Use of this file is governed by the Software License Agreement, located here: https://www.magicleap.com/software-license-agreement-ml2
// Terms and conditions applicable to third-party materials accompanying this distribution may also be found in the top-level NOTICE file appearing herein.
// %COPYRIGHT_END%
// ---------------------------------------------------------------------
// %BANNER_END%

using UnityEngine;
using LightType = ml.zi.LightType;

namespace MagicLeap.ZI
{
    internal struct LightPropertiesData
    {
        public static readonly LightPropertiesData empty = new()
        {
            Type = LightType.Unknown,
            Color = Color.black,
            Intensity = 0,
            Range = 0,
            ConeAngle = 0
        };

        public Color Color;
        public float ConeAngle;
        public float Intensity;
        public float Range;
        public LightType Type;
    }
}
