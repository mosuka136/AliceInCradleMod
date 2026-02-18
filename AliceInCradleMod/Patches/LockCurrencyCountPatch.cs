using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class Patchs
    {
        [HarmonyPatch]
        private class LockCurrencyCountPatch
        {
            private static bool _initialized = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialized)
                    return;

                ConfigManager.LockCurrencyGoldCount.SettingChanged += (s, e) =>
                {
                    if (!ConfigManager.EnableLockCurrencyGoldCount.Value)
                        return;

                    if (UInt32.TryParse(ConfigManager.LockCurrencyGoldCount.Value.ToString(), out var count))
                        SetCurrencyCountPatch.SetCurrencyGoldCount(count);
                };
                ConfigManager.LockCurrencyCraftsCount.SettingChanged += (s, e) =>
                {
                    if (!ConfigManager.EnableLockCurrencyCraftsCount.Value)
                        return;

                    if (UInt32.TryParse(ConfigManager.LockCurrencyCraftsCount.Value.ToString(), out var count))
                        SetCurrencyCountPatch.SetCurrencyCraftsCount(count);
                };
                ConfigManager.LockCurrencyJuiceCount.SettingChanged += (s, e) =>
                {
                    if (!ConfigManager.EnableLockCurrencyJuiceCount.Value)
                        return;

                    if (UInt32.TryParse(ConfigManager.LockCurrencyJuiceCount.Value.ToString(), out var count))
                        SetCurrencyCountPatch.SetCurrencyJuiceCount(count);
                };

                _initialized = true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoinEntry), "Add")]
            static bool AddPrefix(CoinEntry __instance)
            {
                return DealWithCurrencyCount(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoinEntry), "Reduce")]
            static bool ReducePrefix(CoinEntry __instance)
            {
                return DealWithCurrencyCount(__instance);
            }

            private static bool DealWithCurrencyCount(CoinEntry cEntry)
            {
                var ctype = cEntry.ctype;
                if (ctype == CoinStorage.CTYPE.GOLD)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyGoldCount.Value,
                        ConfigManager.LockCurrencyGoldCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.CRAFTS)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyCraftsCount.Value,
                        ConfigManager.LockCurrencyCraftsCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.JUICE)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyJuiceCount.Value,
                        ConfigManager.LockCurrencyJuiceCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE._TEMPORARY)
                {
                }

                return true;
            }

            private static bool DealWithCurrencyCount(bool isEnabled, long lockCount, CoinEntry cEntry)
            {
                if (!isEnabled)
                    return true;

                if (lockCount < 0)
                    return false;

                if (!UInt32.TryParse(lockCount.ToString(), out var lockCountUInt))
                {
                    HLog.Error($"Failed to parse lock count for {cEntry.ctype} currency. Value: {lockCount}");
                    return true;
                }

                uint count = cEntry.Get();
                if (count == lockCountUInt)
                    return false;

                cEntry.Set(lockCountUInt, true);

                return false;
            }
        }
    }
}
