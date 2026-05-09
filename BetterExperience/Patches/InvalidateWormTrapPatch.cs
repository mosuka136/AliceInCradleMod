using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class InvalidateWormTrapPatch
        {
            [HarmonyPatch(typeof(M2WormTrap), "isCovering")]
            public static bool Prefix(ref bool __result)
            {
                try
                {
                    if (ConfigManager.EnableWormTrap.Value)
                        return true;

                    __result = false;

                    HLog.Debug($"{nameof(InvalidateWormTrapPatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(InvalidateWormTrapPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
