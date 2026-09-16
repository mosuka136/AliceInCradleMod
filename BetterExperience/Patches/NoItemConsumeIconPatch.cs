using BetterExperience.BLogSpace;
using HarmonyLib;
using m2d;
using nel;
using System;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 物品超量使用不消耗的图标特效（仅快捷物品栏）。
        /// 快捷栏逐帧重绘，在受保护物品的图标上原位叠加一层同形状的半透明图标，
        /// 色相随时间沿色相环流动（红→黄→绿→青→蓝→品红，约 2.4 秒一圈），
        /// 不同物品按物品键错开相位；判定与扣减拦截完全同源，仅在主背包
        /// 且该判定成立时显示，迷惑替换图标不叠加。
        /// 物品菜单的行网格只在重建时绘制一次、无法逐帧变色，不做特效。
        /// </summary>
        [HarmonyPatch]
        public static class NoItemConsumeIconPatch
        {
            // 快捷物品栏的图标由 drawCell 直接绘制，后缀原位叠加流动色层；
            // 位置与缩放按原版画法：RotaPF(x - w * 0.12f, y + w * 0.12f, iconscale)。
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UseItemSelector), "drawCell",
                new[] { typeof(UseItemSelector.ItCell), typeof(float), typeof(float), typeof(float), typeof(float), typeof(float), typeof(MeshDrawer), typeof(MeshDrawer), typeof(MeshDrawer), typeof(bool), typeof(bool), typeof(bool) })]
            public static void QuickBarDrawCellPostfix(
                UseItemSelector __instance,
                UseItemSelector.ItCell Cl,
                float x,
                float y,
                float iconscale,
                float w,
                float alpha,
                MeshDrawer MdIco)
            {
                try
                {
                    if (MdIco == null || Cl?.Itm == null || !NoItemConsumePatch.IsEnabled())
                        return;

                    var inventory = __instance.IMNG?.getInventory();
                    if (inventory == null)
                        return;
                    // 迷惑状态下原版把图标换成问号，不叠加以免泄露真实物品。
                    var pr = __instance.IMNG.Mp?.getKeyPr() as PR;
                    if (pr != null && pr.Ser.getLevel(SER.CONFUSE) >= 2)
                        return;

                    // 快捷栏逐帧重绘：色相随帧数流动，并按物品键错开相位。
                    float flowHue = X.ANMPT(NoItemConsumeLogic.IconEffectPeriodFrames)
                        + NoItemConsumeLogic.HueFromKey(Cl.Itm.key);
                    DrawOverlay(
                        MdIco, Cl.Itm, inventory, Cl.Itm.getIcon(inventory),
                        flowHue, x - w * 0.12f, y + w * 0.12f, iconscale, alpha);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(QuickBarDrawCellPostfix)}.", ex);
                }
            }

            private static void DrawOverlay(
                MeshDrawer Md,
                NelItem itm,
                ItemStorage inventory,
                int iconIndex,
                float hue,
                float x,
                float y,
                float scale,
                float alpha)
            {
                if (iconIndex < 0 || iconIndex >= MTR.AItemIcon.Length)
                    return;
                if (!NoItemConsumePatch.WouldKeep(inventory, itm, out _))
                    return;

                NoItemConsumeLogic.HueToRgb(hue, out float r, out float g, out float b);
                Md.initForImg(MTRX.IconWhite);
                Md.Col = new Color(r, g, b, alpha * NoItemConsumeLogic.IconEffectOverlayAlpha);
                Md.RotaPF(x, y, scale, scale, F: MTR.AItemIcon[iconIndex]);
            }
        }
    }
}
