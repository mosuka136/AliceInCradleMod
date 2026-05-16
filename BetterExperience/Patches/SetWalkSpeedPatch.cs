using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 按倍率调整玩家移动速度。
        /// move_aim_ex 为 0 时视作无移动输入，补丁不修改结果，避免待机状态出现非零速度。
        /// </summary>
        [HarmonyPatch]
        public class SetWalkSpeedPatch
        {
            private static bool _hasLoggedActivation = false;

            [HarmonyPostfix]
            [HarmonyPatch(typeof(PR), "calcWalkSpeed")]
            public static void Postfix(int move_aim_ex, ref float __result)
            {
                try
                {
                    if (ConfigManager.SetPlayerWalkSpeed.Value <= 0f)
                        return;

                    if (move_aim_ex == 0)
                        return;

                    __result *= ConfigManager.SetPlayerWalkSpeed.Value;

                    if (!_hasLoggedActivation)
                    {
                        HLog.Debug($"{nameof(SetWalkSpeedPatch)} applied.");
                        _hasLoggedActivation = true;
                    }
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(SetWalkSpeedPatch)}", ex);
                }
            }
        }
    }
}
