using BetterExperience.BConfigManager;
using BetterExperience.HotkeyManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        public class FlushStorePatch
        {
            private static Hotkey _flushStoreHotkey;

            [HarmonyPatch]
            public class FrameUpdateBoosterPatch
            {
                [HarmonyPostfix]
                [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
                public static void Postfix()
                {
                    FrameUpdateBooster.Instance.OnFrameUpdate += Update;

                    ConfigManager.FlushAllStoreHotkey.OnValueChanged += (s, e) =>
                    {
                        _flushStoreHotkey = null;
                    };
                }
            }

            public static void Update()
            {
                if (!ConfigManager.EnableFlushAllStore.Value)
                    return;

                if (_flushStoreHotkey == null)
                {
                    _flushStoreHotkey = new Hotkey();
                    var h = ConfigManager.FlushAllStoreHotkey.Value;
                    if (!_flushStoreHotkey.TryParse(h))
                    {
                        HLog.Warn("Invalid Hotkey: " + h);

                        h = "F";
                        _flushStoreHotkey.TryParse(h);
                        HLog.Info("Flush store hotkey set: " + h);
                    }
                }

                if (_flushStoreHotkey != null && _flushStoreHotkey.WasPressedThisFrame())
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
