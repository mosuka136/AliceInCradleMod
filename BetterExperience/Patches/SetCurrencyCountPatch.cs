using BetterExperience.BConfigManager;
using UnityModBase.HClassAttribute;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置或锁定货币数量。
        /// 预加载只在读档后写入一次；锁定模式通过拦截 Add/Reduce 阻止游戏修改数量。
        /// </summary>
        [HarmonyPatch]
        public class SetCurrencyCountPatch
        {
            private static bool _initialized = false;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    if (ConfigManager.SetCurrencyGoldCount.Value1
                        && UInt32.TryParse(ConfigManager.SetCurrencyGoldCount.Value2.ToString(), out var countGold))
                        SetCurrencyGoldCount(countGold);

                    if (ConfigManager.SetCurrencyCraftsCount.Value1
                        && UInt32.TryParse(ConfigManager.SetCurrencyCraftsCount.Value2.ToString(), out var countCrafts))
                        SetCurrencyCraftsCount(countCrafts);

                    if (ConfigManager.SetCurrencyJuiceCount.Value1
                        && UInt32.TryParse(ConfigManager.SetCurrencyJuiceCount.Value2.ToString(), out var countJuice))
                        SetCurrencyJuiceCount(countJuice);
                };

                _initialized = true;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoinEntry), "Add")]
            public static bool AddPrefix(CoinEntry __instance)
            {
                try
                {
                    return DealWithCurrencyCount(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetCurrencyCountPatch)}", ex);
                    return true;
                }
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(CoinEntry), "Reduce")]
            public static bool ReducePrefix(CoinEntry __instance)
            {
                try
                {
                    return DealWithCurrencyCount(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetCurrencyCountPatch)}", ex);
                    return true;
                }
            }

            public static bool DealWithCurrencyCount(CoinEntry cEntry)
            {
                var ctype = cEntry.ctype;
                if (ctype == CoinStorage.CTYPE.GOLD)
                {
                    return DealWithCurrencyCount(ConfigManager.EnableLockCurrencyGoldCount.Value, cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.CRAFTS)
                {
                    return DealWithCurrencyCount(ConfigManager.EnableLockCurrencyCraftsCount.Value, cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.JUICE)
                {
                    return DealWithCurrencyCount(ConfigManager.EnableLockCurrencyJuiceCount.Value, cEntry);
                }

                BLog.Notice($"Unknown currency type: {ctype}. No lock applied.");
                return true;
            }

            public static bool DealWithCurrencyCount(bool isEnabled, CoinEntry cEntry)
            {
                if (!isEnabled)
                    return true;

                BLog.Debug($"{cEntry.ctype} count locked at {cEntry.Get()}.");
                return false;
            }

            public static long GetCurrencyGoldCount()
            {
                return GetCurrencyCount(CoinStorage.CTYPE.GOLD);
            }

            public static long GetCurrencyCraftsCount()
            {
                return GetCurrencyCount(CoinStorage.CTYPE.CRAFTS);
            }

            public static long GetCurrencyJuiceCount()
            {
                return GetCurrencyCount(CoinStorage.CTYPE.JUICE);
            }

            public static void SetCurrencyGoldCount(long count)
            {
                SetCurrencyCount(count, "GOLD", SetCurrencyGoldCount);
            }

            public static void SetCurrencyCraftsCount(long count)
            {
                SetCurrencyCount(count, "CRAFTS", SetCurrencyCraftsCount);
            }

            public static void SetCurrencyJuiceCount(long count)
            {
                SetCurrencyCount(count, "JUICE", SetCurrencyJuiceCount);
            }

            private static long GetCurrencyCount(CoinStorage.CTYPE type)
            {
                var entry = CoinStorage.GetEntry(type);
                return entry == null ? -1L : entry.Get();
            }

            private static void SetCurrencyCount(long count, string currencyName, Action<uint> valueSetter)
            {
                if (count < 0 || count > UInt32.MaxValue)
                {
                    BLog.Debug($"Ignored invalid {currencyName} count: {count}");
                    return;
                }

                valueSetter((uint)count);
            }

            public static void SetCurrencyGoldCount(uint count)
            {
                try
                {
                    var entry = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                    if (entry == null)
                        return;

                    count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;

                    entry[0].Set(count, true);
                    entry[0].Add(0);

                    BLog.Debug($"GOLD count set to: {count}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error while setting GOLD count in {nameof(SetCurrencyCountPatch)}", ex);
                }
            }

            public static void SetCurrencyCraftsCount(uint count)
            {
                try
                {
                    var entry = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                    if (entry == null)
                        return;

                    count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;

                    entry[1].Set(count, true);
                    entry[1].Add(0);

                    BLog.Debug($"CRAFTS count set to: {count}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error while setting CRAFTS count in {nameof(SetCurrencyCountPatch)}", ex);
                }
            }

            public static void SetCurrencyJuiceCount(uint count)
            {
                try
                {
                    var entry = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                    if (entry == null)
                        return;

                    count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;

                    entry[2].Set(count, true);
                    entry[2].Add(0);

                    BLog.Debug($"JUICE count set to: {count}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error while setting JUICE count in {nameof(SetCurrencyCountPatch)}", ex);
                }
            }
        }
    }
}
