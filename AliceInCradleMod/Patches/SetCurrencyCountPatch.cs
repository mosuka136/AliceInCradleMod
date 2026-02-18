using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        internal class SetCurrencyCountPatch
        {
            private static bool _initialized = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(FrameUpdateBooster), nameof(FrameUpdateBooster.Awake))]
            private static void Initialize()
            {
                if (_initialized)
                    return;

                ConfigManager.SetCurrencyGoldCount.Value = -1;
                ConfigManager.SetCurrencyCraftsCount.Value = -1;
                ConfigManager.SetCurrencyJuiceCount.Value = -1;

                ConfigManager.SetCurrencyGoldCount.SettingChanged += (s, e) =>
                {
                    uint count;
                    if (UInt32.TryParse(ConfigManager.SetCurrencyGoldCount.Value.ToString(), out count))
                        SetCurrencyGoldCount(count);
                };
                ConfigManager.SetCurrencyCraftsCount.SettingChanged += (s, e) =>
                {
                    uint count;
                    if (UInt32.TryParse(ConfigManager.SetCurrencyCraftsCount.Value.ToString(), out count))
                        SetCurrencyCraftsCount(count);
                };
                ConfigManager.SetCurrencyJuiceCount.SettingChanged += (s, e) =>
                {
                    uint count;
                    if (UInt32.TryParse(ConfigManager.SetCurrencyJuiceCount.Value.ToString(), out count))
                        SetCurrencyJuiceCount(count);
                };

                _initialized = true;
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
