using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetLootDropRatioPatch
        {
            private static bool _hasLoggedDisableDrop = false;
            private static bool _hasLoggedRatioOverride = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelEnemy), "checkDropChance")]
            public static bool Prefix(NelEnemy __instance)
            {
                if (ConfigManager.SetLootDropRatio.Value < 0f)
                    return true;

                if (ConfigManager.SetLootDropRatio.Value == 0f)
                {
                    if (!_hasLoggedDisableDrop)
                    {
                        HLog.Debug("Loot drop disabled.");
                        _hasLoggedDisableDrop = true;
                    }

                    return false;
                }

                __instance.dropratio1000 = Convert.ToUInt16(__instance.dropratio1000 / ConfigManager.SetLootDropRatio.Value);

                if (!_hasLoggedRatioOverride)
                {
                    HLog.Debug($"Loot drop ratio override applied.");
                    _hasLoggedRatioOverride = true;
                }

                return true;
            }
        }
    }
}
