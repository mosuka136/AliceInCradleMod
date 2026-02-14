using HarmonyLib;
using nel;
using System;

namespace BetterExperience
{
    internal partial class Patchs
    {
        [HarmonyPatch]
        private class LockCurrencyCountPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoinEntry), "Add")]
            static bool AddPrefix(CoinEntry __instance)
            {
                if (!ConfigManager.EnableLockCurrencyCount.Value)
                    return true;

                return DealWithCurrencyCount(__instance);
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoinEntry), "Reduce")]
            static bool ReducePrefix(CoinEntry __instance)
            {
                if (!ConfigManager.EnableLockCurrencyCount.Value)
                    return true;

                return DealWithCurrencyCount(__instance);
            }

            private static bool DealWithCurrencyCount(CoinEntry centry)
            {
                var ctype = centry.ctype;
                if (ctype == CoinStorage.CTYPE.GOLD)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyGoldCount.Value,
                        ConfigManager.LockCurrencyGoldCount.Value,
                        ctype,
                        centry);
                }
                else if (ctype == CoinStorage.CTYPE.CRAFTS)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyCraftsCount.Value,
                        ConfigManager.LockCurrencyCraftsCount.Value,
                        ctype,
                        centry);
                }
                else if (ctype == CoinStorage.CTYPE.JUICE)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyJuiceCount.Value,
                        ConfigManager.LockCurrencyJuiceCount.Value,
                        ctype,
                        centry);
                }
                else if (ctype == CoinStorage.CTYPE._TEMPORARY)
                {
                }

                return true;
            }

            private static bool DealWithCurrencyCount(
                bool isEnabled, long lockCount, CoinStorage.CTYPE type, CoinEntry centry)
            {
                if (!isEnabled)
                    return true;

                if (lockCount < 0)
                    return false;

                if (!UInt32.TryParse(lockCount.ToString(), out var lockCountUInt))
                {
                    HLog.Error($"Failed to parse lock count for {type} currency. Value: {lockCount}");
                    return true;
                }

                uint count = centry.Get();
                if (count == lockCountUInt)
                    return false;

                centry.Set(lockCountUInt, true);

                return false;
            }
        }
    }
}
