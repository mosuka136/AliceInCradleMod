using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class RemovePressDamagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), nameof(PR.applyPressDamage), new Type[] { typeof(NelAttackInfo), typeof(bool), typeof(int) })]
            private static bool Prefix()
            {
                if (ConfigManager.EnablePressDamage.Value)
                    return true;

                return false;
            }
        }
    }
}
