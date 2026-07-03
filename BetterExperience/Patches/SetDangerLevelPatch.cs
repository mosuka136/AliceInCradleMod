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
                    if (ConfigManager.EnablePreloadDangerLevel.Value)
                    {
                        BLog.Debug($"Applying preloaded danger level: {ConfigManager.SetDangerLevel.Value}");
                        SetDangerLevel(ConfigManager.SetDangerLevel.Value);
                    }
                };

                ConfigManager.SetDangerLevel.OnValueChanged += (s, e) =>
                {
                    BLog.Debug($"Danger level config changed: {e}");
                    SetDangerLevel(e);
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

                    var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                    if (sg == null)
                    {
                        BLog.Notice("SceneGame not found while applying danger level.");
                        return;
                    }

                    var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                    if (m2d == null)
                    {
                        BLog.Notice("NelM2DBase not found while applying danger level.");
                        return;
                    }

                    Traverse.Create(m2d.NightCon).Field("dlevel").SetValue(level);
                    m2d.NightCon.showNightLevelAdditionUI(true);

                    BLog.Debug($"Danger level set to {level}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetDangerLevel)}.", ex);
                }
            }
        }
    }
}
