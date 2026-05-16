using BetterExperience.BConfigManager;
using BetterExperience.HClassAttribute;
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
                    if (UInt32.TryParse(e.ToString(), out var count))
                        SetCurrencyGoldCount(count);
                };
                ConfigManager.SetCurrencyCraftsCount.OnValueChanged += (s, e) =>
                {
                    if (UInt32.TryParse(e.ToString(), out var count))
                        SetCurrencyCraftsCount(count);
                };
                ConfigManager.SetCurrencyJuiceCount.OnValueChanged += (s, e) =>
                {
                    if (UInt32.TryParse(e.ToString(), out var count))
                        SetCurrencyJuiceCount(count);
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
                    HLog.Error($"Unexpected error in {nameof(SetCurrencyCountPatch)}", ex);
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
                    HLog.Error($"Unexpected error in {nameof(SetCurrencyCountPatch)}", ex);
                    return true;
                }
            }

            public static bool DealWithCurrencyCount(CoinEntry cEntry)
            {
                var ctype = cEntry.ctype;
                if (ctype == CoinStorage.CTYPE.GOLD)
                {
                    HLog.Debug($"Set GOLD count to: {ConfigManager.SetCurrencyGoldCount.Value}");
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyGoldCount.Value,
                        ConfigManager.SetCurrencyGoldCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.CRAFTS)
                {
                    HLog.Debug($"Set CRAFTS count to: {ConfigManager.SetCurrencyCraftsCount.Value}");
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyCraftsCount.Value,
                        ConfigManager.SetCurrencyCraftsCount.Value,
                        cEntry);
                }
                else if (ctype == CoinStorage.CTYPE.JUICE)
                {
                    HLog.Debug($"Set JUICE count to: {ConfigManager.SetCurrencyJuiceCount.Value}");
                    return DealWithCurrencyCount(
                        ConfigManager.EnableLockCurrencyJuiceCount.Value,
                        ConfigManager.SetCurrencyJuiceCount.Value,
                        cEntry);
                }

                HLog.Notice($"Unknown currency type: {ctype}. No lock applied.");
                return true;
            }

            public static bool DealWithCurrencyCount(bool isEnabled, long count, CoinEntry cEntry)
            {
                if (!isEnabled)
                    return true;

                // 负数表示只冻结当前数量，不主动把货币改成配置值。
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
                try
                {
                    var entry = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                    if (entry == null)
                        return;

                    count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;

                    entry[0].Set(count, true);
                    entry[0].Add(0);

                    HLog.Debug($"GOLD count set to: {count}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error while setting GOLD count in {nameof(SetCurrencyCountPatch)}", ex);
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

                    HLog.Debug($"CRAFTS count set to: {count}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error while setting CRAFTS count in {nameof(SetCurrencyCountPatch)}", ex);
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

                    HLog.Debug($"JUICE count set to: {count}");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error while setting JUICE count in {nameof(SetCurrencyCountPatch)}", ex);
                }
            }
        }
    }
}
