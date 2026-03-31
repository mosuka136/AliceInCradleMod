using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class SetDangerLevelPatch
        {
            private static bool _initialized = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
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
