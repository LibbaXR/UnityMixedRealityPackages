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
    internal static class Vec3fExtension
    {
        public static void FromQuat(this Quaternionf self, Quaternion quat)
        {
            self.x = quat.x;
            self.y = quat.y;
            self.z = quat.z;
            self.w = quat.w;
        }

        public static void FromVec3(this Vec3f self, Vector3 vector)
        {
            self.x = vector.x;
            self.y = vector.y;
            self.z = vector.z;
        }

        public static Color ToColor(this Vec3f self)
        {
            return new Color(self.x, self.y, self.z);
        }

        public static Quaternion ToQuat(this Quaternionf self)
        {
            Quaternion quat;
            quat.x = self.x;
            quat.y = self.y;
            quat.z = self.z;
            quat.w = self.w;
            return quat;
        }

        public static Quaternionf ToQuatf(this Quaternion self)
        {
            return new Quaternionf {x = self.x, y = self.y, z = self.z, w = self.w};
        }

        public static Vector3 ToVec3(this Vec3f self)
        {
            Vector3 vector;
            vector.x = self.x;
            vector.y = self.y;
            vector.z = self.z;
            return vector;
        }

        public static Vec3f ToVec3f(this Vector3 self)
        {
            return new Vec3f {x = self.x, y = self.y, z = self.z};
        }

        public static Vec3f ToVec3f(this Color color)
        {
            return new Vec3f {x = color.r, y = color.g, z = color.b};
        }
    }
}
