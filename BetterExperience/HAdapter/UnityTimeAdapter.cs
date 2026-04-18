using UnityEngine;

namespace BetterExperience.HAdapter
{
    public static class UnityTimeAdapter
    {
        public static float DeltaTime => Time.deltaTime;
        public static float UnscaledDeltaTime => Time.unscaledDeltaTime;
        public static float RealtimeSinceStartup => Time.realtimeSinceStartup;
        public static int FrameCount => Time.frameCount;
    }
}
