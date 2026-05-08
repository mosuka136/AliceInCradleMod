using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
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

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadPlayerMaxSatiety.Value)
                    {
                        HLog.Debug($"Applying preloaded max satiety: {ConfigManager.SetPlayerMaxSatiety.Value}");
                        SetMaxSatiety(ConfigManager.SetPlayerMaxSatiety.Value);
                    }
                    else
                    {
                        if (_maxSatiety > 0)
                        {
                            HLog.Debug($"Restoring original max satiety: {_maxSatiety}");
                            SetMaxSatiety(_maxSatiety);
                        }
                    }
                };

                ConfigManager.SetPlayerMaxSatiety.OnValueChanged += (s, e) =>
                {
                   HLog.Debug($"Max satiety config changed: {e}");
                   SetMaxSatiety(e);
                };

                _initialized = true;
                HLog.Debug("Max satiety patch initialized.");
            }

            public static void SetMaxSatiety(int maxSatiety)
            {
                if (maxSatiety <= 0)
                {
                    HLog.Debug($"Ignored invalid max satiety value: {maxSatiety}");
                    return;
                }

                var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
                if (pr == null)
                {
                    HLog.Notice("Player instance not found while applying max satiety.");
                    return;
                }

                if (pr.MyStomach == null)
                {
                    HLog.Notice("Player stomach data not found while applying max satiety.");
                    return;
                }

                if (_maxSatiety < 0)
                {
                    _maxSatiety = pr.MyStomach.cost_max;
                    HLog.Debug($"Captured original max satiety: {_maxSatiety}");
                }

                pr.MyStomach.cost_max = maxSatiety;
                HLog.Debug($"Player max satiety set to {maxSatiety}");
            }
        }
    }
}
