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
        /// 首次应用时缓存原值；如果读档后预加载关闭，则尝试恢复缓存的原始最大饱食度。
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
                    if (ConfigManager.EnablePreloadPlayerMaxSatiety.Value)
                    {
                        BLog.Debug($"Applying preloaded max satiety: {ConfigManager.SetPlayerMaxSatiety.Value}");
                        SetMaxSatiety(ConfigManager.SetPlayerMaxSatiety.Value);
                    }
                    else
                    {
                        if (_maxSatiety > 0)
                        {
                            BLog.Debug($"Restoring original max satiety: {_maxSatiety}");
                            SetMaxSatiety(_maxSatiety);
                        }
                    }
                };

                ConfigManager.SetPlayerMaxSatiety.OnValueChanged += (s, e) =>
                {
                    BLog.Debug($"Max satiety config changed: {e}");
                    SetMaxSatiety(e);
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

                    var pr = UnityEngine.Object.FindAnyObjectByType<PR>();
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
        }
    }
}
