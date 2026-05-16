using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 关闭溺水效果时重置缺氧和水中计时字段。
        /// 这里使用 Postfix 是为了让游戏原逻辑先更新周边状态，再覆盖会造成伤害的累计值。
        /// </summary>
        [HarmonyPatch]
        public class DisableDrowningPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(M2PrMistApplier), "applyGasDamage")]
            public static void Postfix(M2PrMistApplier __instance)
            {
                try
                {
                    if (ConfigManager.EnableDrowning.Value)
                        return;

                    Traverse.Create(__instance).Field("o2_point").SetValue(99.9f);
                    Traverse.Create(__instance).Field("t_water").SetValue(0f);
                    HLog.Debug($"{nameof(DisableDrowningPatch)} applied.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(DisableDrowningPatch)}", ex);
                }
            }
        }
    }
}
