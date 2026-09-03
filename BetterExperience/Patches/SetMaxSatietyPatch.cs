using BetterExperience.BConfigManager;
using UnityModBase.HClassAttribute;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置玩家最大饱食度。
        /// 每次读档后重新捕获当前存档的原始值，避免沿用其他存档的基准。
        /// </summary>
        [HarmonyPatch]
        public class SetMaxSatietyPatch
        {
            private static bool _initialized = false;
            // -1 表示尚未捕获过游戏原始最大饱食度。
            private static int _maxSatiety = -1;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    // 原始值属于当前存档，读档后必须重新捕获，不能沿用上一个存档的基准。
                    _maxSatiety = -1;

                    if (ConfigManager.SetPlayerMaxSatiety.Value1)
                    {
                        BLog.Debug($"Applying preloaded max satiety: {ConfigManager.SetPlayerMaxSatiety.Value2}");
                        SetMaxSatiety(ConfigManager.SetPlayerMaxSatiety.Value2);
                    }
                };

                _initialized = true;
                BLog.Debug("Max satiety patch initialized.");
            }

            public static void SetMaxSatiety(int maxSatiety)
            {
                try
                {
                    if (maxSatiety <= 0)
                    {
                        BLog.Debug($"Ignored invalid max satiety value: {maxSatiety}");
                        return;
                    }

                    var pr = GetPR();
                    if (pr == null)
                    {
                        BLog.Notice("Player instance not found while applying max satiety.");
                        return;
                    }

                    if (pr.MyStomach == null)
                    {
                        BLog.Notice("Player stomach data not found while applying max satiety.");
                        return;
                    }

                    if (_maxSatiety < 0)
                    {
                        _maxSatiety = pr.MyStomach.cost_max;
                        BLog.Debug($"Captured original max satiety: {_maxSatiety}");
                    }

                    pr.MyStomach.cost_max = maxSatiety;
                    BLog.Debug($"Player max satiety set to {maxSatiety}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetMaxSatiety)}.", ex);
                }
            }

            public static int GetMaxSatiety()
            {
                var stomach = GetPR()?.MyStomach;
                return stomach == null ? -1 : stomach.cost_max;
            }
        }
    }
}
