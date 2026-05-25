using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using HarmonyLib;
using m2d;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 阻止玩家进入受击伤害处理。
        /// 该补丁位于 M2PrADmg.applyDamage 入口，比单独拦截 HP/MP/EP 更早，适合关闭敌人攻击造成的整套受击流程。
        /// </summary>
        [HarmonyPatch]
        public class UnableBeingAttackedPatch
        {
            private static bool _hasLoggedActivation = false;

            [HarmonyPatch(typeof(M2PrADmg), "applyDamage",
                new Type[] {
                    typeof(NelAttackInfo),
                    typeof(HITTYPE),
                    typeof(bool),
                    typeof(string),
                    typeof(bool),
                    typeof(bool) },
                new ArgumentType[] {
                    ArgumentType.Normal,
                    ArgumentType.Ref,
                    ArgumentType.Normal,
                    ArgumentType.Normal,
                    ArgumentType.Normal,
                    ArgumentType.Normal})]
            public static bool Prefix()
            {
                try
                {
                    if (ConfigManager.EnableBeingAttacked.Value)
                        return true;

                    if (!_hasLoggedActivation)
                    {
                        HLog.Debug("Player damage reception disabled.");
                        _hasLoggedActivation = true;
                    }

                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(UnableBeingAttackedPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
