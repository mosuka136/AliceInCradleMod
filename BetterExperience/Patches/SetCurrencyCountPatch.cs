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
        /// 设置有两个入口：读档后按双值配置预加载一次，或经实时控制界面即时修改；
        /// 锁定模式通过 Prefix 拦截 CoinEntry.Add/Reduce，把数量冻结在当前值，不主动改写。
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
                    // 双值配置以 long 存储设置值，游戏接口使用 uint；经字符串解析一并完成负数与超界过滤，非法值跳过不应用。
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

            /// <summary>
            /// CoinEntry.Add 的 Prefix：对应货币启用锁定时返回 false 跳过原方法，阻止数量增加。
            /// </summary>
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

            /// <summary>
            /// CoinEntry.Reduce 的 Prefix：对应货币启用锁定时返回 false 跳过原方法，阻止数量减少。
            /// </summary>
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

            /// <summary>
            /// 按货币类型应用锁定判定，作为 Add/Reduce 两个 Prefix 的公共入口。
            /// 返回值直接作为 Prefix 结果：true 放行原方法，false 拦截。
            /// </summary>
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

            /// <summary>
            /// 锁定判定：启用时返回 false 拦截本次变更，把数量冻结在当前值（具体数值由设置入口另行写入）；
            /// 未启用时返回 true 放行原方法。
            /// </summary>
            public static bool DealWithCurrencyCount(bool isEnabled, CoinEntry cEntry)
            {
                if (!isEnabled)
                    return true;

                BLog.Debug($"{cEntry.ctype} count locked at {cEntry.Get()}.");
                return false;
            }

            // 以下 long 重载供实时控制界面调用（配置与控制条目均以 long 表示货币值），校验范围后转发到 uint 重载。
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

            /// <summary>
            /// 读取指定货币的当前数量，供实时控制界面显示；对应条目不存在时返回 -1 占位。
            /// </summary>
            private static long GetCurrencyCount(CoinStorage.CTYPE type)
            {
                var entry = CoinStorage.GetEntry(type);
                return entry == null ? -1L : entry.Get();
            }

            /// <summary>
            /// 校验货币数量在 uint 范围内后转发给 <paramref name="valueSetter"/>；负数或超界值忽略并记录日志。
            /// </summary>
            private static void SetCurrencyCount(long count, string currencyName, Action<uint> valueSetter)
            {
                if (count < 0 || count > UInt32.MaxValue)
                {
                    BLog.Debug($"Ignored invalid {currencyName} count: {count}");
                    return;
                }

                valueSetter((uint)count);
            }

            // 以下 uint 重载执行实际写入：Aentry 是 CoinStorage 的静态私有数组，
            // 索引 0/1/2 固定对应 GOLD/CRAFTS/JUICE。Set 写入后再调用一次 Add(0)，
            // 借游戏自身的数量变更流程刷新货币显示；若该货币已启用锁定，这次 Add(0) 会被本补丁拦截，不影响已写入的数值。
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
