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
            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelEnemy), "checkDropChance")]
            public static bool Prefix(NelEnemy __instance)
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
