using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 覆盖转轮速度衰减参数。
        /// 只接受 0 到 1 的配置值，超出范围时交给游戏原逻辑处理。
        /// </summary>
        [HarmonyPatch]
        public class SetReelSpeedPatch
        {
            private static bool _hasLoggedActivation = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(ReelExecuter), nameof(ReelExecuter.fineSpeed))]
            public static void SetReelSpeed(ref float reduce_level)
            {
                try
                {
                    if (ConfigManager.SetReelSpeed.Value < 0f || ConfigManager.SetReelSpeed.Value > 1f)
                        return;

                    reduce_level = ConfigManager.SetReelSpeed.Value;

                    if (!_hasLoggedActivation)
                    {
                        BLog.Debug($"{nameof(SetReelSpeedPatch)} applied.");
                        _hasLoggedActivation = true;
                    }
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetReelSpeedPatch)}", ex);
                }
            }
        }
    }
}
