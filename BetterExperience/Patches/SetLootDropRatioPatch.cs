using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 修改敌人掉落战利品概率倍率。
        /// 配置为 -1 时不接管，0 时禁用掉落，大于 0 时通过调整游戏内部 dropratio1000 影响原判定。
        /// </summary>
        [HarmonyPatch]
        public class SetLootDropRatioPatch
        {
            private static bool _hasLoggedDisableDrop = false;
            private static bool _hasLoggedRatioOverride = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelEnemy), "checkDropChance")]
            public static bool Prefix(NelEnemy __instance)
            {
                try
                {
                    if (ConfigManager.SetLootDropRatio.Value < 0f)
                        return true;

                    if (ConfigManager.SetLootDropRatio.Value == 0f)
                    {
                        if (!_hasLoggedDisableDrop)
                        {
                            HLog.Debug("Loot drop disabled.");
                            _hasLoggedDisableDrop = true;
                        }

                        return false;
                    }

                    __instance.dropratio1000 = Convert.ToUInt16(__instance.dropratio1000 / ConfigManager.SetLootDropRatio.Value);

                    if (!_hasLoggedRatioOverride)
                    {
                        HLog.Debug($"Loot drop ratio override applied.");
                        _hasLoggedRatioOverride = true;
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetLootDropRatioPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
