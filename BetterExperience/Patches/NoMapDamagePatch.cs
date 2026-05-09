using BetterExperience.BConfigManager;
using HarmonyLib;
using m2d;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class NoMapDamagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "applyDamageFromMap")]
            public static bool Prefix(ref AttackInfo __result)
            {
                try
                {
                    if (ConfigManager.EnableMapDamage.Value)
                        return true;

                    __result = null;

                    HLog.Debug($"{nameof(NoMapDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(NoMapDamagePatch)}", ex);
                    return true;
                }
            }
        }
    }
}
