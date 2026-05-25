using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 禁用玩家因与敌人接触等判定触发的倒地检查。
        /// 配置关闭倒地时跳过 <c>checkEnemySink</c>，其余玩家状态逻辑保持原样。
        /// </summary>
        [HarmonyPatch]
        public class RemoveFallingToGroundPatch
        {
            private static bool _hasLoggedActivation = false;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(PR), "checkEnemySink")]
            public static bool Prefix()
            {
                try
                {
                    if(ConfigManager.EnableFallingToGround.Value)
                        return true;

                    if (!_hasLoggedActivation)
                    {
                        HLog.Debug($"{nameof(RemoveFallingToGroundPatch)} applied.");
                        _hasLoggedActivation = true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveFallingToGroundPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
