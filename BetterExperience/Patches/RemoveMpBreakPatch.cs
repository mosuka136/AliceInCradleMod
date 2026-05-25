using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 阻止 MP 槽破裂。
        /// 配置关闭破裂时清空破裂计数和破裂槽数组，并跳过原伤害逻辑。
        /// </summary>
        [HarmonyPatch]
        public class RemoveMpBreakPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(MpGaugeBreaker), "gageDamage")]
            public static bool Prefix(MpGaugeBreaker __instance)
            {
                try
                {
                    if (ConfigManager.EnableMpBreak.Value)
                        return true;

                    var mg = Traverse.Create(__instance);
                    mg.Field("break_cnt").SetValue(0);
                    var Agage_breaked = mg.Field("Agage_breaked").GetValue<byte[]>();
                    for (int i = 0; i < Agage_breaked.Length; i++)
                    {
                        Agage_breaked[i] = 0;
                    }

                    HLog.Debug($"{nameof(RemoveMpBreakPatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveMpBreakPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
