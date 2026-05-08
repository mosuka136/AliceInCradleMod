using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;

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
                if (ConfigManager.EnableWormTrap.Value)
                    return true;

                __result = false;

                HLog.Debug($"{nameof(InvalidateWormTrapPatch)} applied.");
                return false;
            }
        }
    }
}
