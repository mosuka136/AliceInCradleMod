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
        /// 夜间限定魔物阵常开。
        /// 原版夜间限定（only_night）的魔物阵白天显示为关闭且无法开启，
        /// 判定集中在 M2LpSummon.noon_donot_show（无子类遮蔽、仅内部两处消费：
        /// need_set_effect 的显示状态与 cannot_open_summoner 的开启拦截）。
        /// 此处归零后白天照常显示为开启并可进入；敌人的等级与掉落
        /// 仍按真实昼夜（危险度）计算，公会任务阵与剧情锁定阵不受影响。
        /// </summary>
        [HarmonyPatch]
        public static class NightSummonerAlwaysOpenPatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(M2LpSummon), "noon_donot_show", MethodType.Getter)]
            public static void NoonDonotShowPostfix(ref bool __result)
            {
                try
                {
                    if (ConfigManager.EnableNightSummonerAlwaysOpen?.Value == true)
                        __result = false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(NoonDonotShowPostfix)}.", ex);
                }
            }
        }
    }
}
