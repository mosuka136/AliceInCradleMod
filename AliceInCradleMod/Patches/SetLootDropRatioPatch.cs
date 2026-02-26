using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SetLootDropRatioPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelEnemy), "checkDropChance")]
            private static bool Prefix(NelEnemy __instance)
            {
                if (ConfigManager.SetLootDropRatio.Value < 0f)
                    return true;

                if (ConfigManager.SetLootDropRatio.Value == 0f)
                    return false;

                __instance.dropratio1000 = Convert.ToUInt16(__instance.dropratio1000 / ConfigManager.SetLootDropRatio.Value);

                return true;
            }
        }
    }
}
