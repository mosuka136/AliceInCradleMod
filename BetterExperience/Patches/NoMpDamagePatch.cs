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
        /// 阻止玩家 MP 伤害。
        /// 目标方法含有 out/ref 参数，补丁通过显式签名匹配，配置开启时跳过原方法。
        /// </summary>
        [HarmonyPatch]
        public class NoMpDamagePatch
        {
            [HarmonyPatch(
                typeof(PR),
                "applyMpDamage",
                new Type[] {
                    typeof(float),
                    typeof(int),
                    typeof(bool),
                    typeof(AttackInfo),
                    typeof(bool),
                    typeof(bool),
                    typeof(bool) },
                new ArgumentType[] {
                    ArgumentType.Out,
                    ArgumentType.Normal,
                    ArgumentType.Normal,
                    ArgumentType.Normal,
                    ArgumentType.Normal,
                    ArgumentType.Normal,
                    ArgumentType.Normal }
                )]
            public static bool Prefix()
            {
                try
                {
                    if (!ConfigManager.EnableNoMpDamage.Value)
                        return true;

                    HLog.Debug($"{nameof(NoMpDamagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(NoMpDamagePatch)}.", ex);
                    return true;
                }
            }
        }
    }
}
