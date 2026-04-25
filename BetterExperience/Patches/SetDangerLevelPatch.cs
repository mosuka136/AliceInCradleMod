using BetterExperience.BConfigManager;
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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadDangerLevel.Value)
                        SetDangerLevel(ConfigManager.SetDangerLevel.Value);
                };

                ConfigManager.SetDangerLevel.OnValueChanged += (s, e) =>
                {
                    SetDangerLevel(ConfigManager.SetDangerLevel.Value);
                };

                _initialized = true;
            }

            public static void SetDangerLevel(int level)
            {
                if (level < 0)
                    return;

                var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                if (sg == null)
                    return;

                var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                if (m2d == null)
                    return;

                Traverse.Create(m2d.NightCon).Field("dlevel").SetValue(level);
                m2d.NightCon.showNightLevelAdditionUI(true);
            }
        }
    }
}
