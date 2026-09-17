using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using nel.smnp;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 出现即秒杀开关：在敌人状态机运转（runPost）时判死，清空 HP 并切入死亡状态，
        /// 之后由游戏正常执行 runDie（掉落、战斗点击杀计数、移除并刷新下一波）。
        /// 不能在 appear 时立即判死：出生流程随后会 changeState(SUMMONED) 覆盖死亡状态，
        /// 留下打不还手也打不死的残血敌人并卡住下一波刷新。
        /// 救援任务魔物、召唤小游戏单位、塔防单位与剧情事件敌人不处理，避免卡住任务和玩法。
        /// 攻击造成大伤害的秒杀方式见 <see cref="KillEnemies"/>。
        /// </summary>
        [HarmonyPatch]
        public class KillEnemiesOnSpawnPatch
        {
            private static bool _enabled;

            internal static bool GetKillEnemiesOnSpawn() => _enabled;

            internal static void SetKillEnemiesOnSpawn(bool enabled)
            {
                try
                {
                    _enabled = enabled;
                    BLog.Debug($"Kill enemies on spawn toggled: {enabled}.");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetKillEnemiesOnSpawn)}.", ex);
                }
            }

            // 开启后场上已存在的敌人也会在下一帧被击杀，与"出现即秒杀"的表现保持一致。
            [HarmonyPostfix]
            [HarmonyPatch(typeof(NelEnemy), nameof(NelEnemy.runPost))]
            public static void KillOnRun(NelEnemy __instance)
            {
                try
                {
                    if (!_enabled || !__instance.is_alive || ShouldSkip(__instance))
                        return;

                    __instance.initDeath();
                    BLog.Debug($"Killed enemy on spawn: {__instance.id}.");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(KillOnRun)}.", ex);
                }
            }

            private static bool ShouldSkip(NelEnemy enemy)
            {
                return enemy.Summoner is SummonerPlayerForRescue
                    || enemy.Summoner is nel.mgm.smncr.SummonerPlayerForCreator
                    || enemy.is_towerdefence
                    || enemy is MvNelNNEAListener.NelNNpcEventAssign;
            }
        }
    }
}
