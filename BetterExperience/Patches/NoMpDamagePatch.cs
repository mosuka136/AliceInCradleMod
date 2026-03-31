using BetterExperience.BepConfigManager;
using HarmonyLib;
using m2d;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
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
                if (!ConfigManager.EnableNoMpDamage.Value)
                    return true;

                return false;
            }
        }
    }
}
