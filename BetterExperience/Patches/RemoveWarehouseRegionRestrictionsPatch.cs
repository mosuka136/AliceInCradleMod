using BetterExperience.BepConfigManager;
using HarmonyLib;
using nel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        [HarmonyPatch]
        public class RemoveWarehouseRegionRestrictionsPatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelM2DBase), nameof(NelM2DBase.canAccesableToHouseInventory))]
            public static bool NelM2DBase_canAccesableToHouseInventory_Prefix(ref bool __result)
            {
                if (!ConfigManager.EnableAccessWarehouseAnywhere.Value)
                    return true;

                __result = true;
                return false;
            }
        }
    }
}
