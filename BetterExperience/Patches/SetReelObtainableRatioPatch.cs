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
        /// 卷轴可获取数量倍率。
        /// 原版战斗后可抽取的物品卷轴数在 UiDangerousViewer.fineCurrentUiObtainable
        /// 汇总，且被钳制在战斗次数附近（单场战斗最多约 3 个）；
        /// 此处在其后按配置倍率调整最终数量，绕过战斗次数钳制。
        /// 待领的转轮类型栈会自动随机补充，发放与显示循环均按该数量驱动；
        /// 结果按 0–255 收敛（以 byte 存储），已获得的卷轴不受影响。
        /// 倍率小于 0 时保持原版。
        /// </summary>
        [HarmonyPatch]
        public static class SetReelObtainableRatioPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiDangerousViewer), nameof(UiDangerousViewer.fineCurrentUiObtainable))]
            public static void FineObtainablePostfix(UiDangerousViewer __instance)
            {
                try
                {
                    float ratio = ConfigManager.SetReelObtainableRatio?.Value ?? -1f;
                    if (ratio < 0f)
                        return;

                    var field = Traverse.Create(__instance).Field("current_ui_obtainable");
                    int scaled = SetReelObtainableRatioLogic.ApplyRatio(
                        field.GetValue<byte>(), ratio);
                    field.SetValue((byte)scaled);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(FineObtainablePostfix)}.", ex);
                }
            }
        }
    }
}
