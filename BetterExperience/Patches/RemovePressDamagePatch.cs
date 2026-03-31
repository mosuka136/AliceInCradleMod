using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class RemovePressDamagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), nameof(PR.applyPressDamage), new Type[] { typeof(NelAttackInfo), typeof(bool), typeof(int) })]
            public static bool Prefix()
            {
                if (ConfigManager.EnablePressDamage.Value)
                    return true;

                return false;
            }
        }
    }
}
