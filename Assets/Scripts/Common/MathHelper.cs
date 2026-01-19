using System;

namespace Assets.Scripts.Common
{
    public static class MathHelper
    {
        public const float Epsilon = 0.00001f;

        public static float Clamp(float value, float min, float max)
        {
            if (value < min) return min;
            if (value > max) return max;
            return value;
        }

        public static float Clamp01(float value)
        {
            return Clamp(value, 0f, 1f);
        }

        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }

        public static bool Approximately(float a, float b)
        {
            return Math.Abs(a - b) < Epsilon;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }
    }
}