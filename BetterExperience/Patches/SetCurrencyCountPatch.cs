using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class SetCurrencyCountPatch
        {
            private static bool _initialized = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameAttributePatchManager.Instance.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.EnablePreloadCurrencyGoldCount.Value
                        && UInt32.TryParse(ConfigManager.SetCurrencyGoldCount.Value.ToString(), out var countGold))
                        SetCurrencyGoldCount(countGold);

                    if (ConfigManager.EnablePreloadCurrencyCraftsCount.Value
                        && UInt32.TryParse(ConfigManager.SetCurrencyCraftsCount.Value.ToString(), out var countCrafts))
                        SetCurrencyCraftsCount(countCrafts);

                    if (ConfigManager.EnablePreloadCurrencyJuiceCount.Value
                        && UInt32.TryParse(ConfigManager.SetCurrencyJuiceCount.Value.ToString(), out var countJuice))
                        SetCurrencyJuiceCount(countJuice);
                };

                ConfigManager.SetCurrencyGoldCount.OnValueChanged += (s, e) =>
                {
                    if (UInt32.TryParse(ConfigManager.SetCurrencyGoldCount.Value.ToString(), out var count))
                        SetCurrencyGoldCount(count);
                };
                ConfigManager.SetCurrencyCraftsCount.OnValueChanged += (s, e) =>
                {
                    if (UInt32.TryParse(ConfigManager.SetCurrencyCraftsCount.Value.ToString(), out var count))
                        SetCurrencyCraftsCount(count);
                };
                ConfigManager.SetCurrencyJuiceCount.OnValueChanged += (s, e) =>
                {
                    if (UInt32.TryParse(ConfigManager.SetCurrencyJuiceCount.Value.ToString(), out var count))
                        SetCurrencyJuiceCount(count);
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

            public static bool DealWithCurrencyCount(CoinEntry cEntry)
            {
                var ctype = cEntry.ctype;
                if (ctype == CoinStorage.CTYPE.GOLD)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyGoldCount.Value,
                        ConfigManager.SetCurrencyGoldCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.CRAFTS)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyCraftsCount.Value,
                        ConfigManager.SetCurrencyCraftsCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.JUICE)
                {
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyJuiceCount.Value,
                        ConfigManager.SetCurrencyJuiceCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE._TEMPORARY)
                {
                }

                return true;
            }

            public static bool DealWithCurrencyCount(bool isEnabled, long count, CoinEntry cEntry)
            {
                if (!isEnabled)
                    return true;

                if (count < 0)
                    return false;

                if (!UInt32.TryParse(count.ToString(), out var countUInt))
                {
                    HLog.Error($"Failed to parse lock count for {cEntry.ctype} currency. Value: {count}");
                    return true;
                }

                if (cEntry.Get() == countUInt)
                    return false;

                cEntry.Set(countUInt, true);

                return false;
            }

            public static void SetCurrencyGoldCount(uint count)
            {
                var Aentry = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                if (Aentry == null)
                    return;

                count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;

                Aentry[0].Set(count, true);
                Aentry[0].Add(0);
            }

            public static void SetCurrencyCraftsCount(uint count)
            {
                var Aentry = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                if (Aentry == null)
                    return;

                count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;

                Aentry[1].Set(count, true);
                Aentry[1].Add(0);
            }

            public static void SetCurrencyJuiceCount(uint count)
            {
                var Aentry = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                if (Aentry == null)
                    return;

                count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;

                Aentry[2].Set(count, true);
                Aentry[2].Add(0);
            }
        }
    }
}
