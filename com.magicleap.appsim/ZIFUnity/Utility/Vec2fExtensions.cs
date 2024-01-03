using ml.zi;
using UnityEngine;

namespace MagicLeap.ZI
{
    internal static class Vec2fExtensions
    {
        public static void FromVec3(this Vec2f self, Vector2 vector)
        {
            self.x = vector.x;
            self.y = vector.y;
        }
        
        public static Vector2 ToVec2(this Vec2f self)
        {
            Vector2 vector;
            vector.x = self.x;
            vector.y = self.y;
            return vector;
        }

        public static Vec2f ToVec2f(this Vector2 self)
        {
            return new Vec2f {x = self.x, y = self.y};
        }
    }
}
