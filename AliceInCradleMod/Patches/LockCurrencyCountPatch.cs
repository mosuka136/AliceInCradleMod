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
            [HarmonyPatch(typeof(CoinStorage), "addCount", new Type[] { typeof(int), typeof(CoinStorage.CTYPE), typeof(bool) })]
            static bool AddCountPrefix(CoinStorage.CTYPE ctype)
            {
                if (!ConfigManager.EnableLockCurrencyCount.Value)
                    return true;

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
            [HarmonyPatch(typeof(CoinStorage), "reduceCount", new Type[] { typeof(int), typeof(CoinStorage.CTYPE), typeof(bool) })]
            static bool ReduceCountPrefix(CoinStorage.CTYPE ctype)
            {
                if (!ConfigManager.EnableLockCurrencyCount.Value)
                    return true;

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

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiItemStore), "confirmCheckout")]
            static void ConfirmCheckoutPostfix(UiItemStore __instance, UiItemStore.StoreResult Res)
            {
                if (!ConfigManager.EnableLockCurrencyCount.Value || !ConfigManager.EnableLockCurrencyGoldCount.Value)
                    return;

                if (__instance == null)
                {
                    HLog.Error("UiItemStore instance is null.");
                    return;
                }

                var centry = Traverse.Create(__instance).Field<CoinEntry>("CEntry").Value;
                if (centry == null)
                {
                    HLog.Error("Failed to access UiItemStore.CEntry.");
                    return;
                }

                var lockCountLong = ConfigManager.LockCurrencyGoldCount.Value;
                if (lockCountLong < 0)
                {
                    if (Res.money_addition < 0)
                        centry.Add(-Res.money_addition);
                    else
                        centry.Reduce(Res.money_addition);

                    return;
                }

                if (!UInt32.TryParse(lockCountLong.ToString(), out var lockCount))
                {
                    HLog.Error($"Failed to parse lock count for GOLD currency. Value: {lockCountLong}");
                    return;
                }

                centry.Set(lockCount, true);
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
