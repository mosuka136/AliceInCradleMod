using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using m2d;
using nel;
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 流浪商人的地图常显。
        /// 世界地图上夜莺不再要求携带铃铛即可见，其余商人（咖啡师、提尔德、木偶商人）
        /// 以各自颜色的可出现范围圈标出当前位置；剧情未解锁的商人仍不显示。
        /// </summary>
        [HarmonyPatch]
        public static class WanderingNpcPresencePatch
        {
            // nel.gm.UiGMCMap 是 internal 类型，绘制方法只能经名字反射定位。
            private static readonly MethodInfo DrawNightingaleMethod = AccessTools.Method(
                AccessTools.TypeByName("nel.gm.UiGMCMap"),
                "FnDrawNightingale");

            [HarmonyTargetMethods]
            public static IEnumerable<MethodBase> TargetMethods()
            {
                if (DrawNightingaleMethod == null)
                {
                    BLog.Error("FnDrawNightingale not found on nel.gm.UiGMCMap.");
                    yield break;
                }

                yield return DrawNightingaleMethod;
            }

            // 夜莺图标原版由 see_nightingale 门控（要求携带铃铛），这里改为仅要求剧情解锁。
            [HarmonyPrefix]
            public static void FnDrawNightingalePrefix(object __instance)
            {
                try
                {
                    if (ConfigManager.EnableAlwaysShowWanderingNpcOnMap?.Value != true)
                        return;

                    var nightingale = GetM2D()?.WDR?.getNightingale();
                    if (nightingale == null)
                        return;

                    Traverse.Create(__instance).Field("see_nightingale").SetValue(
                        WanderingNpcPresenceLogic.ShouldShowOnMap(true, nightingale.isEnable()));
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(FnDrawNightingalePrefix)}.", ex);
                }
            }

            // 其余商人没有地图图标，用与夜莺相同的可出现范围圈按商人配色标出。
            [HarmonyPostfix]
            public static void FnDrawNightingalePostfix(
                ButtonSkinWholeMapArea WmSkin,
                MeshDrawer MdIco,
                float blink_alpha,
                float mappos_x,
                float mappos_y,
                float cell_size)
            {
                try
                {
                    if (ConfigManager.EnableAlwaysShowWanderingNpcOnMap?.Value != true)
                        return;

                    var m2d = GetM2D();
                    var wdr = m2d?.WDR;
                    if (wdr == null || WmSkin.getWholeMapTarget() != m2d.WM.CurWM)
                        return;

                    DrawMerchantCircle(wdr, SummonWanderingNpcKind.CoffeeMaker, MdIco, blink_alpha, mappos_x, mappos_y, cell_size);
                    DrawMerchantCircle(wdr, SummonWanderingNpcKind.Tilde, MdIco, blink_alpha, mappos_x, mappos_y, cell_size);
                    DrawMerchantCircle(wdr, SummonWanderingNpcKind.Puppet, MdIco, blink_alpha, mappos_x, mappos_y, cell_size);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(FnDrawNightingalePostfix)}.", ex);
                }
            }

            private static void DrawMerchantCircle(
                WanderingManager wdr,
                SummonWanderingNpcKind kind,
                MeshDrawer MdIco,
                float blink_alpha,
                float mappos_x,
                float mappos_y,
                float cell_size)
            {
                var npc = wdr.Get(WanderingNpcForceLogic.ToGameType(kind));
                if (!WanderingNpcPresenceLogic.ShouldShowOnMap(true, npc.isEnable()))
                    return;

                Vector2 position = npc.getPosition();
                float x = WholeMapItem.map2meshx(position.x, mappos_x, cell_size);
                float y = WholeMapItem.map2meshy(position.y, mappos_y, cell_size);
                var color = WanderingNpcPresenceLogic.CircleColor(kind);

                // 原版 drawPointCurs(POINT_CURS.SUN_S) 内部会强制覆盖为白色，
                // 这里按其同款画法（白色点状贴图绕圈）直接染色绘制；
                // 半径使用所有商人一致的固定格数，只作位置标记，不表达实际可出现范围。
                MdIco.initForImg(MTRX.IconWhite);
                MdIco.Col = new Color(color.R, color.G, color.B, blink_alpha);
                float radius = cell_size * WanderingNpcPresenceLogic.CircleRadiusCells
                    * (X.COSIT(100f) * 0.125f + 1f);
                float phase = 1f - X.ANMPT(400);
                for (int i = 0; i < WanderingNpcPresenceLogic.CirclePointCount; i++)
                {
                    float angle = (phase + i / (float)WanderingNpcPresenceLogic.CirclePointCount) * 6.2831854f;
                    MdIco.RotaPF(x + radius * X.Cos(angle), y + radius * X.Sin(angle), F: MTR.MeshCursPoint);
                }
            }
        }

        /// <summary>
        /// 流浪商人必定出现。
        /// 把商人出现率固定为 1：checkBench 的判定为随机数 &lt; 出现率，必定成立；
        /// 原版调用点均使用默认参数（读实例字段），写入公有字段即可稳定生效。
        /// 地图加载时长椅点在商人游走范围内必定生成，单地图单商人与剧情地图黑名单等
        /// 原版规则保持不变。
        /// </summary>
        [HarmonyPatch]
        public static class WanderingNpcAlwaysAppearPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(WanderingNPC), nameof(WanderingNPC.checkBench))]
            public static void CheckBenchPrefix(WanderingNPC __instance)
            {
                try
                {
                    if (ConfigManager.EnableWanderingNpcAlwaysAppear?.Value != true)
                        return;

                    __instance.appear_ratio = 1f;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(CheckBenchPrefix)}.", ex);
                }
            }
        }
    }
}
