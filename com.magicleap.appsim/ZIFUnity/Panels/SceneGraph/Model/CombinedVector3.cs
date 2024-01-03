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
    internal struct CombinedVector3
    {
        public static CombinedVector3 zero = new()
        {
            vector = Vector3.zero,
            isCombinedX = true,
            isCombinedY = true,
            isCombinedZ = true
        };

        public bool isCombinedX;
        public bool isCombinedY;
        public bool isCombinedZ;
        public Vector3 vector;

        public float X => vector.x;
        public float Y => vector.y;
        public float Z => vector.z;

        public CombinedVector3(Vector3 vector, bool isCombinedX = true, bool isCombinedY = true, bool isCombinedZ = true)
        {
            this.vector = vector;
            this.isCombinedX = isCombinedX;
            this.isCombinedY = isCombinedY;
            this.isCombinedZ = isCombinedZ;
        }

        public void Combine(Vector3 newValue)
        {
            float FloatPropertyEpsilon = 0.0001f;

            isCombinedX &= !(Math.Abs(newValue.x - vector.x) > FloatPropertyEpsilon);
            isCombinedY &= !(Math.Abs(newValue.y - vector.y) > FloatPropertyEpsilon);
            isCombinedZ &= !(Math.Abs(newValue.z - vector.z) > FloatPropertyEpsilon);
        }
    }
}
