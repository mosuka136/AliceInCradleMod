using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using System;
using UnityModBase.HClassAttribute;

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

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
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

                    pr.MyStomach.cost_max = maxSatiety;
                    BLog.Debug($"Player max satiety set to {maxSatiety}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetMaxSatiety)}.", ex);
                }
            }

            /// <summary>
            /// 读取当前最大饱食度，供实时控制界面显示；玩家或胃部数据未加载时返回 -1 占位。
            /// </summary>
            public static int GetMaxSatiety()
            {
                var stomach = GetPR()?.MyStomach;
                return stomach == null ? -1 : stomach.cost_max;
            }
        }
    }
}
