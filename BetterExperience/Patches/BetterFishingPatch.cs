using BetterExperience.BConfigManager;
using HarmonyLib;
using nel.mgm.fis;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class BetterFishingPatch
        {
            private const float IncreaseRatio = 1.6f;
            private const float DecreaseRatio = 0.8f;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(FisCatchFishMarker), nameof(FisCatchFishMarker.activate))]
            public static void BetterFishingPrefix(ref FishData.MKInfo _Mki)
            {
                if (!ConfigManager.EnableBetterFishing.Value)
                    return;

                var t_switch = _Mki.t_switch_max - _Mki.t_switch_min;
                _Mki.t_switch_min *= IncreaseRatio;
                _Mki.t_switch_max = _Mki.t_switch_min + t_switch;

                var jump01 = _Mki.jump01_max - _Mki.jump01_min;
                _Mki.jump01_min *= DecreaseRatio;
                _Mki.jump01_max = _Mki.jump01_min + jump01;

                var switchspeed01 = _Mki.switchspeed01_max - _Mki.switchspeed01_min;
                _Mki.switchspeed01_min *= DecreaseRatio;
                _Mki.switchspeed01_max = _Mki.switchspeed01_min + switchspeed01;

                var move_fast_range = _Mki.move_fast_range_max - _Mki.move_fast_range_min;
                _Mki.move_fast_range_min *= DecreaseRatio;
                _Mki.move_fast_range_max = _Mki.move_fast_range_min + move_fast_range;

                _Mki.switch_another_ratio *= DecreaseRatio;

                _Mki.switch_another_lock *= IncreaseRatio;

                _Mki.force_goto_reverse_ratio *= DecreaseRatio;

                _Mki.switch_slower_ratio *= IncreaseRatio;

                var switch_slower_multiple = _Mki.switch_slower_multiple_max - _Mki.switch_slower_multiple_min;
                _Mki.switch_slower_multiple_min *= IncreaseRatio;
                _Mki.switch_slower_multiple_max = _Mki.switch_slower_multiple_min + switch_slower_multiple;

                _Mki.first_speed_ratio *= DecreaseRatio;

                var fistjump_multiple = _Mki.fistjump_multiple_max - _Mki.fistjump_multiple_min;
                _Mki.fistjump_multiple_min *= DecreaseRatio;
                _Mki.fistjump_multiple_max = _Mki.fistjump_multiple_min + fistjump_multiple;
            }
        }
    }
}
