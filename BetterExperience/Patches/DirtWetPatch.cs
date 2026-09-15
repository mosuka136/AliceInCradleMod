using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using System.Reflection;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 不沾污渍、不浸湿。配置持久化，关掉后新的污渍和浸湿恢复原版。
        /// 冻结/石化/蛛网贴图仍走异常系统；已有污渍需椅子清洗。
        /// </summary>
        internal static class DirtWetAssist
        {
            private static bool _loggedDirt;
            private static bool _loggedWet;

            internal static bool NoDirtOn => IsOn(ConfigManager.EnableNoDirtStains);
            internal static bool NoWetOn => IsOn(ConfigManager.EnableNoWetten);

            internal static bool ShouldBlockDirt(BetoInfo info)
            {
                return info != null && DirtWetLogic.ShouldSkipDirt(NoDirtOn, info.type);
            }

            internal static void RevertJuiceWet(PR pr)
            {
                if (!NoWetOn || pr == null || pr.BetoMng == null || !pr.BetoMng.wetten)
                    return;
                pr.BetoMng.setWetten(pr, false);
                LogOnce(ref _loggedWet, "no wetten");
            }

            internal static void LogDirtOnce()
            {
                LogOnce(ref _loggedDirt, "no dirt");
            }

            internal static void LogWetOnce()
            {
                LogOnce(ref _loggedWet, "no wetten");
            }

            private static bool IsOn(ConfigEntry<bool> entry)
            {
                return ConfigManager.EnableBetterExperience?.Value == true && entry?.Value == true;
            }

            private static void LogOnce(ref bool logged, string what)
            {
                if (logged)
                    return;
                BLog.Debug($"{nameof(DirtWetAssist)} applied: {what}.");
                logged = true;
            }
        }

        /// <summary>
        /// 拦截 BetobetoManager.Check(BetoInfo)，普通污渍不再叠加上去。
        /// </summary>
        [HarmonyPatch]
        public class NoDirtStainsPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(BetobetoManager), nameof(BetobetoManager.Check), new Type[] { typeof(BetoInfo), typeof(bool), typeof(bool) })]
            public static bool Prefix(BetoInfo B)
            {
                try
                {
                    if (!DirtWetAssist.ShouldBlockDirt(B))
                        return true;
                    DirtWetAssist.LogDirtOnce();
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(NoDirtStainsPatch)}", ex);
                    return true;
                }
            }
        }

        /// <summary>
        /// 拦截 setWetten(true)。允许椅子等路径把浸湿关掉。
        /// </summary>
        [HarmonyPatch]
        public class NoWettenPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(BetobetoManager), nameof(BetobetoManager.setWetten))]
            public static bool Prefix(bool f)
            {
                try
                {
                    if (!DirtWetLogic.ShouldSkipWetten(DirtWetAssist.NoWetOn, f))
                        return true;
                    DirtWetAssist.LogWetOnce();
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(NoWettenPatch)}", ex);
                    return true;
                }
            }
        }

        /// <summary>
        /// 体液溅射直接写 wetten 字段，补一次回写。M2PrAJuiceCon 为游戏 internal 类型。
        /// </summary>
        [HarmonyPatch]
        public class NoJuiceWettenPatch
        {
            public static bool Prepare()
            {
                return TargetMethod() != null;
            }

            public static MethodBase TargetMethod()
            {
                var type = AccessTools.TypeByName("nel.M2PrAJuiceCon");
                return type == null ? null : AccessTools.Method(type, "splashNoelJuiceEffect");
            }

            [HarmonyPostfix]
            public static void Postfix(object __instance)
            {
                try
                {
                    if (!(__instance is M2PrAssistant assistant))
                        return;
                    DirtWetAssist.RevertJuiceWet(assistant.Pr);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(NoJuiceWettenPatch)}", ex);
                }
            }
        }
    }
}
