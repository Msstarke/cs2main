using System;
using System.Numerics;

namespace cs2aim.Utils
{
    public static class MathHelpers
    {
        public static Vector2 CalculateAngles(Vector3 from, Vector3 to)
        {
            float dx = to.X - from.X;
            float dy = to.Y - from.Y;
            float dz = to.Z - from.Z;

            float yaw = (float)(Math.Atan2(dy, dx) * (180.0 / Math.PI));
            float dist = (float)Math.Sqrt(dx * dx + dy * dy);
            float pitch = -(float)(Math.Atan2(dz, dist) * (180.0 / Math.PI));

            return new Vector2(yaw, pitch);
        }

        public static float NormalizeAngle(float angle)
        {
            while (angle > 180f) angle -= 360f;
            while (angle < -180f) angle += 360f;
            return angle;
        }

        public static float Clamp(float value, float min, float max)
            => value < min ? min : (value > max ? max : value);
    }
}
