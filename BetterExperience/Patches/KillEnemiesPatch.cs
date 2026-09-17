using BetterExperience.BLogSpace;
using System;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 秒杀敌人开关，直接读写游戏的调试全能标志 X.DEBUGMIGHTY。
        /// 标志开启时玩家造成的伤害会被放大为敌人最大 HP 的 64 倍（NelEnemy.applyDamage），
        /// 击杀流程（掉落、经验、战斗统计）保持正常；静态标志跨场景保持，不写入存档。
        /// </summary>
        internal static class KillEnemies
        {
            internal static bool GetKillEnemies() => X.DEBUGMIGHTY;

            internal static void SetKillEnemies(bool enabled)
            {
                try
                {
                    X.DEBUGMIGHTY = enabled;
                    BLog.Debug($"Kill enemies toggled: {enabled}.");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetKillEnemies)}.", ex);
                }
            }
        }
    }
}
