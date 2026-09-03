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
        /// 设置危险度。
        /// 配置值小于 0 时视为不覆盖；应用后会调用游戏 UI 更新方法刷新显示。
        /// </summary>
        [HarmonyPatch]
        public class SetDangerLevelPatch
        {
            private static bool _initialized = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.SetDangerLevel.Value1)
                    {
                        BLog.Debug($"Applying preloaded danger level: {ConfigManager.SetDangerLevel.Value2}");
                        SetDangerLevel(ConfigManager.SetDangerLevel.Value2);
                    }
                };

                _initialized = true;
                BLog.Debug("Danger level patch initialized.");
            }

            public static void SetDangerLevel(int level)
            {
                try
                {
                    if (level < 0)
                    {
                        BLog.Debug($"Ignored invalid danger level: {level}");
                        return;
                    }

                    var nightController = GetNightController();
                    if (nightController == null)
                    {
                        BLog.Notice("NightController not found while applying danger level.");
                        return;
                    }

                    Traverse.Create(nightController).Field("dlevel").SetValue(level);
                    nightController.showNightLevelAdditionUI(true);

                    BLog.Debug($"Danger level set to {level}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetDangerLevel)}.", ex);
                }
            }

            public static int GetDangerLevel()
            {
                var nightController = GetNightController();
                return nightController == null
                    ? -1
                    : nightController.getDangerMeterVal(real: true, raw: true);
            }
        }
    }
}
