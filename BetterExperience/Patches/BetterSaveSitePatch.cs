using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 放宽当前地图和玩家状态对保存行为的限制。
        /// 两个 Prefix 都通过设置返回值并跳过原方法实现，因此只在配置开启时接管判定。
        /// </summary>
        [HarmonyPatch]
        public class BetterSaveSitePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelM2DBase), "canSaveInCurMap")]
            public static bool CanSaveInCurMapPrefix(NelM2DBase __instance, ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableBetterSaveSite.Value)
                        return true;

                    if (__instance.curMap == null)
                    {
                        HLog.Notice($"curMap is null.");
                        return true;
                    }

                    __result = true;

                    HLog.Debug($"{nameof(CanSaveInCurMapPrefix)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(BetterSaveSitePatch)}", ex);
                    return true;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "canSave", new Type[] { typeof(bool) })]
            public static bool CanSavePrefix(ref bool __result)
            {
                try
                {
                    if (!ConfigManager.EnableBetterSaveSite.Value)
                        return true;

                    __result = true;

                    HLog.Debug($"{nameof(CanSavePrefix)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(BetterSaveSitePatch)}", ex);
                    return true;
                }
            }
        }
    }
}
