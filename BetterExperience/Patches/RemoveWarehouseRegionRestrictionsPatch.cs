using BetterExperience.BConfigManager;
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
            public static bool CanAccesableToHouseInventoryPrefix(ref bool __result)
            {
                if (!ConfigManager.EnableAccessWarehouseAnywhere.Value)
                    return true;

                __result = true;

                HLog.Debug($"{nameof(RemoveWarehouseRegionRestrictionsPatch)} applied.");
                return false;
            }
        }
    }
}
