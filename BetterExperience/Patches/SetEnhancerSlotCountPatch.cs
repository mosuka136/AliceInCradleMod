using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetEnhancerSlotCountPatch
        {
            private static bool _initialized = false;
            private static bool _isChanging = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadEnhancerSlotCount.Value)
                        SetEnhancerSlotCount();
                };

                ConfigManager.SetEnhancerSlotCount.OnValueChanged += (s, e) =>
                {
                    SetEnhancerSlotCount();
                };

                _initialized = true;
            }

            public static void SetEnhancerSlotCount()
            {
                var sg = UnityEngine.Object.FindAnyObjectByType<SceneGame>();
                if (sg == null)
                    return;

                var m2d = Traverse.Create(sg).Field("M2D").GetValue<NelM2DBase>();
                if (m2d == null)
                    return;

                if (m2d.IMNG == null)
                    return;

                var sp = Traverse.Create(m2d.IMNG).Field("StPrecious").GetValue<ItemStorage>();
                if (sp == null)
                    return;

                var se = Traverse.Create(m2d.IMNG).Field("StEnhancer").GetValue<ItemStorage>();
                if (se == null)
                    return;

                _isChanging = true;
                // ENHA.fineEnhancerStorage方法会调用ItemStorage.getCount方法
                ENHA.fineEnhancerStorage(sp, se);
                _isChanging = false;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ItemStorage), nameof(ItemStorage.getCount), new Type[] { typeof(NelItem), typeof(int) })]
            public static bool GetCountPrefix(NelItem Data, ref int __result)
            {
                if (NelItem.GetById("enhancer_slot") != Data)
                    return true;

                if (!_isChanging || ConfigManager.SetEnhancerSlotCount.Value < 0)
                    return true;

                __result = ConfigManager.SetEnhancerSlotCount.Value;
                return false;
            }
        }
    }
}
