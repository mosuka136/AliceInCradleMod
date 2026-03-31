using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetMaxSatietyPatch
        {
            private static bool _initialized = false;
            private static int _maxSatiety = -1;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadPlayerMaxSatiety.Value)
                        SetMaxSatiety(ConfigManager.SetPlayerMaxSatiety.Value);
                    else
                    {
                        if (_maxSatiety > 0)
                            SetMaxSatiety(_maxSatiety);
                    }
                };

                ConfigManager.SetPlayerMaxSatiety.OnValueChanged += (s, e) =>
                {
                   SetMaxSatiety(ConfigManager.SetPlayerMaxSatiety.Value);
                };

                _initialized = true;
            }

            public static void SetMaxSatiety(int maxSatiety)
            {
                if (maxSatiety <= 0)
                    return;

                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                    return;

                if (pr.MyStomach == null)
                    return;

                if (_maxSatiety < 0)
                    _maxSatiety = pr.MyStomach.cost_max;

                pr.MyStomach.cost_max = maxSatiety;
            }
        }
    }
}
