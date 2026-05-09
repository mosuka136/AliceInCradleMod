using BetterExperience.BConfigManager;
using HarmonyLib;
using m2d;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
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
