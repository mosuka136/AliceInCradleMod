using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
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
                        HLog.Debug($"Applying preloaded danger level: {ConfigManager.SetDangerLevel.Value}");
                        SetDangerLevel(ConfigManager.SetDangerLevel.Value);
                    }
                };

                ConfigManager.SetDangerLevel.OnValueChanged += (s, e) =>
                {
                    HLog.Debug($"Danger level config changed: {e}");
                    SetDangerLevel(e);
                };

                _initialized = true;
                HLog.Debug("Danger level patch initialized.");
            }

            public static void SetDangerLevel(int level)
            {
                if (level < 0)
                {
                    HLog.Debug($"Ignored invalid danger level: {level}");
                    return;
                }

                var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                if (sg == null)
                {
                    HLog.Notice("SceneGame not found while applying danger level.");
                    return;
                }

                var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                if (m2d == null)
                {
                    HLog.Notice("NelM2DBase not found while applying danger level.");
                    return;
                }

                Traverse.Create(m2d.NightCon).Field("dlevel").SetValue(level);
                m2d.NightCon.showNightLevelAdditionUI(true);

                HLog.Debug($"Danger level set to {level}");
            }
        }
    }
}
