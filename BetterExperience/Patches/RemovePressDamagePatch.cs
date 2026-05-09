using BetterExperience.BConfigManager;
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
                try
                {
                    if (ConfigManager.EnablePressDamage.Value)
                        return true;

                    HLog.Debug($"{nameof(RemovePressDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemovePressDamagePatch)}", ex);
                    return true;
                }
            }
        }
    }
}
