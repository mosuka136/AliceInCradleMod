using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class NoEpDamagePatch
        {
            [HarmonyPatch(typeof(EpManager), "applyEpDamage")]
            public static bool Prefix(ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableNoEpDamage.Value)
                        return true;

                    __result = false;

                    HLog.Debug($"{nameof(NoEpDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(NoEpDamagePatch)}", ex);
                    return true;
                }
            }
        }
    }
}
