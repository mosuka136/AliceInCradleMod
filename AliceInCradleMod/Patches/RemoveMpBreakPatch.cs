using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class RemoveMpBreakPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MpGaugeBreaker), "gageDamage")]
            private static bool Prefix(MpGaugeBreaker __instance)
            {
                if (ConfigManager.EnableMpBreak.Value)
                    return true;

                try
                {
                    var mg = Traverse.Create(__instance);
                    mg.Field("break_cnt").SetValue(0);
                    var Agage_breaked = mg.Field("Agage_breaked").GetValue<byte[]>();
                    for (int i = 0; i < Agage_breaked.Length; i++)
                    {
                        Agage_breaked[i] = 0;
                    }
                }
                catch (Exception ex)
                {
                    HLog.Error($"Failed to patch {nameof(RemoveMpBreakPatch)}", ex);
                }

                return false;
            }
        }
    }
}
