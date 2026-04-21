using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        public class FlushStorePatch
        {
            [HarmonyPatch]
            public class FrameUpdateBoosterPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
                public static void Postfix()
                {
                    FrameUpdateBooster.Instance.OnFrameUpdate += Update;
                }
            }

            public static void Update()
            {
                if (!ConfigManager.EnableFlushAllStore.Value)
                    return;

                if (ConfigManager.FlushAllStoreHotkey.Value.WasPressedThisFrame())
                {
                    try
                    {
                        StoreManager.FlushAll();
                    }
                    catch (Exception ex)
                    {
                        HLog.Error("FlushAllStore failed!", ex);
                    }

                    HLog.Warn("Flushed all store!");
                }
            }
        }
    }
}
