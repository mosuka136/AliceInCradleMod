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
        /// 阻止败北画面出现，改为让玩家原地恢复继续游戏。
        /// 原版在败北旗标激活时调用 GAMEOVER.activate 创建败北 UI（含全败场景与任务中止）；
        /// 这里跳过该方法并复用原版续关路径 PR.recoverFromGameOver（HP 提到至少 1、
        /// 切换到恢复状态），等价于自动选择了“继续”。
        /// recoverFromGameOver 末尾的 recheckAll 在 HP 恢复后不会再激活败北旗标；
        /// 重入保护仅作兜底。
        /// </summary>
        [HarmonyPatch]
        public static class PreventGameOverPatch
        {
            private static bool _recovering;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(GAMEOVER), nameof(GAMEOVER.activate))]
            public static bool ActivatePrefix()
            {
                try
                {
                    if (ConfigManager.EnablePreventGameOver?.Value != true)
                        return true;

                    if (_recovering)
                        return false;

                    var player = GetPR();
                    if (player == null)
                        return true;

                    _recovering = true;
                    try
                    {
                        BLog.Debug($"{nameof(PreventGameOverPatch)} applied.");
                        player.recoverFromGameOver();
                    }
                    finally
                    {
                        _recovering = false;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ActivatePrefix)}.", ex);
                    return true;
                }
            }
        }
    }
}
