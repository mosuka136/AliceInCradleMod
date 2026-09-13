using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using UnityModBase.HClassAttribute;
using UnityModBase.HConfigSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置或锁定货币数量。
        /// 设置有两个入口：读档后按双值配置预加载一次，或经实时控制界面即时修改；
        /// 锁定模式通过 Prefix 拦截 CoinEntry.Add/Reduce，把数量冻结在当前值，不主动改写。
        /// 酒吧积分写入时会把生涯获得量抬到不低于当前值，以便累计满 20000 后仍能解锁无限外带。
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
                    ApplyPreload(ConfigManager.SetCurrencyGoldCount, SetCurrencyGoldCount);
                    ApplyPreload(ConfigManager.SetCurrencyCraftsCount, SetCurrencyCraftsCount);
                    ApplyPreload(ConfigManager.SetCurrencyJuiceCount, SetCurrencyJuiceCount);
                    ApplyPreload(ConfigManager.SetCurrencyBarScoreCount, SetCurrencyBarScoreCount);
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
                if (cEntry == null)
                    return true;

                if (!TryGetLockFlag(cEntry.ctype, out var locked))
                {
                    BLog.Notice($"Unknown currency type: {cEntry.ctype}. No lock applied.");
                    return true;
                }

                return DealWithCurrencyCount(locked, cEntry);
            }

            /// <summary>
            /// 识别可锁定的货币类型并读出对应锁定开关。配置尚未绑定时视为未锁定。
            /// </summary>
            internal static bool TryGetLockFlag(CoinStorage.CTYPE ctype, out bool locked)
            {
                locked = false;
                switch (ctype)
                {
                    case CoinStorage.CTYPE.GOLD:
                        locked = ConfigManager.EnableLockCurrencyGoldCount?.Value == true;
                        return true;
                    case CoinStorage.CTYPE.CRAFTS:
                        locked = ConfigManager.EnableLockCurrencyCraftsCount?.Value == true;
                        return true;
                    case CoinStorage.CTYPE.JUICE:
                        locked = ConfigManager.EnableLockCurrencyJuiceCount?.Value == true;
                        return true;
                    case CoinStorage.CTYPE.BAR_SCORE:
                        locked = ConfigManager.EnableLockCurrencyBarScoreCount?.Value == true;
                        return true;
                    default:
                        return false;
                }
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

            public static long GetCurrencyBarScoreCount()
            {
                return GetCurrencyCount(CoinStorage.CTYPE.BAR_SCORE);
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

            public static void SetCurrencyBarScoreCount(long count)
            {
                SetCurrencyCount(count, "BAR_SCORE", SetCurrencyBarScoreCount);
            }

            /// <summary>
            /// 读取指定货币的当前数量，供实时控制界面显示；对应条目不存在时返回 -1 占位。
            /// </summary>
            private static long GetCurrencyCount(CoinStorage.CTYPE type)
            {
                if (type < CoinStorage.CTYPE.GOLD || type >= CoinStorage.CTYPE._MAX)
                    return -1L;

                var entry = CoinStorage.GetEntry(type);
                return entry == null ? -1L : entry.Get();
            }

            /// <summary>
            /// 校验货币数量在 uint 范围内后转发给 <paramref name="valueSetter"/>；负数或超界值忽略并记录日志。
            /// </summary>
            private static void SetCurrencyCount(long count, string currencyName, Action<uint> valueSetter)
            {
                if (!TryConvertCount(count, out var value))
                {
                    BLog.Debug($"Ignored invalid {currencyName} count: {count}");
                    return;
                }

                valueSetter(value);
            }

            internal static bool TryConvertCount(long count, out uint value)
            {
                if (count < 0 || count > uint.MaxValue)
                {
                    value = 0;
                    return false;
                }

                value = (uint)count;
                return true;
            }

            // 以下 uint 重载执行实际写入：Aentry 是 CoinStorage 的静态私有数组，
            // 索引 0/1/2/3 固定对应 GOLD/CRAFTS/JUICE/BAR_SCORE。Set 写入后再调用一次 Add(0)，
            // 借游戏自身的数量变更流程刷新货币显示；若该货币已启用锁定，这次 Add(0) 会被本补丁拦截，不影响已写入的数值。
            public static void SetCurrencyGoldCount(uint count)
            {
                SetCurrencyCount(count, CoinStorage.CTYPE.GOLD);
            }

            public static void SetCurrencyCraftsCount(uint count)
            {
                SetCurrencyCount(count, CoinStorage.CTYPE.CRAFTS);
            }

            public static void SetCurrencyJuiceCount(uint count)
            {
                SetCurrencyCount(count, CoinStorage.CTYPE.JUICE);
            }

            public static void SetCurrencyBarScoreCount(uint count)
            {
                SetCurrencyCount(count, CoinStorage.CTYPE.BAR_SCORE);
            }

            private static void SetCurrencyCount(uint count, CoinStorage.CTYPE type)
            {
                try
                {
                    var entries = Traverse.Create(typeof(CoinStorage)).Field("Aentry").GetValue<CoinEntry[]>();
                    int index = (int)type;
                    if (entries == null || index < 0 || index >= entries.Length)
                        return;

                    var entry = entries[index];
                    if (entry == null)
                        return;

                    count = WriteCount(entry, count, ShouldRaiseObtain(type));
                    entry.Add(0);

                    BLog.Debug($"{type} count set to: {count}");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error while setting {type} count in {nameof(SetCurrencyCountPatch)}", ex);
                }
            }

            internal static bool ShouldRaiseObtain(CoinStorage.CTYPE type)
            {
                return type == CoinStorage.CTYPE.BAR_SCORE;
            }

            /// <summary>
            /// 写入当前余额；酒吧积分还会把 <see cref="CoinEntry.total_obtain"/> 抬到不低于当前值。
            /// </summary>
            internal static uint WriteCount(CoinEntry entry, uint count, bool raiseObtain)
            {
                count = count > CoinEntry.MAX_COUNT ? CoinEntry.MAX_COUNT : count;
                entry.Set(count, true);
                if (raiseObtain && entry.total_obtain < count)
                    entry.total_obtain = count;
                return count;
            }

            private static void ApplyPreload(ConfigEntry<bool, long> entry, Action<uint> setter)
            {
                if (entry != null
                    && entry.Value1
                    && uint.TryParse(entry.Value2.ToString(), out var count))
                    setter(count);
            }
        }
    }
}
