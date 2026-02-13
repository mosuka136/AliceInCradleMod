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

                var ctype = __instance.ctype;
                if (ctype == CoinStorage.CTYPE.GOLD)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyGoldCount.Value,
                        ConfigManager.LockCurrencyGoldCount.Value,
                        ctype);
                }
                else if (ctype == CoinStorage.CTYPE.CRAFTS)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyCraftsCount.Value,
                        ConfigManager.LockCurrencyCraftsCount.Value,
                        ctype);
                }
                else if (ctype == CoinStorage.CTYPE.JUICE)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyJuiceCount.Value,
                        ConfigManager.LockCurrencyJuiceCount.Value,
                        ctype);
                }

                return true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoinEntry), "Reduce")]
            static bool ReducePrefix(CoinEntry __instance)
            {
                if (!ConfigManager.EnableLockCurrencyCount.Value)
                    return true;

                var ctype = __instance.ctype;
                if (ctype == CoinStorage.CTYPE.GOLD)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyGoldCount.Value,
                        ConfigManager.LockCurrencyGoldCount.Value,
                        ctype);
                }
                else if (ctype == CoinStorage.CTYPE.CRAFTS)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyCraftsCount.Value,
                        ConfigManager.LockCurrencyCraftsCount.Value,
                        ctype);
                }
                else if (ctype == CoinStorage.CTYPE.JUICE)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyJuiceCount.Value,
                        ConfigManager.LockCurrencyJuiceCount.Value,
                        ctype);
                }

                return true;
            }

            private static bool DealWithCurrencyCount(bool isEnabled, long lockCount, CoinStorage.CTYPE type)
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

                uint count = CoinStorage.getCount(type);
                if (count == lockCountUInt)
                    return false;

                var aentry = Traverse.Create(typeof(CoinStorage)).Field<CoinEntry[]>("Aentry").Value;
                if (aentry == null)
                {
                    HLog.Error($"Failed to access CoinStorage.Aentry for {type} currency.");
                    return true;
                }

                aentry[(int)type].Set(lockCountUInt, true);

                return false;
            }
        }
    }
}
