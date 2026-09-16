using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 魔力草恢复时间上限。
        /// 原版采集后恢复计时在 SplashManaInit 一处赋值：普通 2400–3000 帧
        /// （约 40–50 秒）、农场小游戏 1320 帧、战后安全状态 6000 帧。
        /// 此处在其后按配置秒数封顶该计时——不影响发光、粒子与魔法飞溅，
        /// 只缩短等待；配置为 -1 时完全保持原版。
        /// </summary>
        [HarmonyPatch]
        public static class ManaWeedRechargePatch
        {
            // 私有重载：SplashManaInit(MANA_HIT, float, float, float, float, float, float, bool)。
            [HarmonyPostfix]
            [HarmonyPatch(typeof(M2ManaWeed), "SplashManaInit",
                new[] { typeof(MANA_HIT), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(bool) })]
            public static void SplashInitPostfix(M2ManaWeed __instance)
            {
                try
                {
                    float seconds = ConfigManager.SetManaWeedRechargeSeconds?.Value
                        ?? ManaWeedRechargeLogic.SecondsDisabled;
                    float capFrames = ManaWeedRechargeLogic.ResolveRechargeFrames(seconds);
                    if (capFrames < 0f)
                        return;

                    var timeField = Traverse.Create(__instance).Field("time");
                    timeField.SetValue(ManaWeedRechargeLogic.CapRechargeTime(
                        timeField.GetValue<float>(), capFrames));
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SplashInitPostfix)}.", ex);
                }
            }
        }
    }
}
