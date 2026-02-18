using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class Patchs
    {
        [HarmonyPatch]
        private class SetMaxSatietyPatch
        {
            private static bool _initialized = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialized)
                    return;

                ConfigManager.SetPlayerMaxSatiety.Value = -1;

                ConfigManager.SetPlayerMaxSatiety.SettingChanged += (s, e) =>
                {
                   SetMaxSatiety(ConfigManager.SetPlayerMaxSatiety.Value);
                };

                _initialized = true;
            }

            public static void SetMaxSatiety(int maxSatiety)
            {
                if (maxSatiety <= 0)
                    return;

                var pr = UnityEngine.Object.FindObjectOfType<PR>();
                if (pr == null)
                    return;

                if (pr.MyStomach == null)
                    return;

                pr.MyStomach.cost_max = maxSatiety;
            }
        }
    }
}
