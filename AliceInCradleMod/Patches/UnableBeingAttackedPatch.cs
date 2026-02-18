using HarmonyLib;
using m2d;
using nel;
using System;

namespace BetterExperience.Patches
{
    internal partial class HPatches
    {
        [HarmonyPatch]
        private class UnableBeingAttackedPatch
        {
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
            static bool Prefix()
            {
                if (ConfigManager.EnableBeingAttacked.Value)
                    return true;

                return false;
            }
        }
    }
}
