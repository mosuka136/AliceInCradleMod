using UnityEngine;

namespace BetterExperience.HAdapter
{
    public static class MathHelper
    {
        public static float Clamp(float value, float min, float max)
        {
            return Mathf.Clamp(value, min, max);
        }

        public static bool Approximately(float a, float b)
        {
            return Mathf.Approximately(a, b);
        }

        public static float Round(float value)
        {
            return Mathf.Round(value);
        }

        public static float Min(float a, float b)
        {
            return a < b ? a : b;
        }

        public static float Max(float a, float b)
        {
            return a > b ? a : b;
        }
    }
}
