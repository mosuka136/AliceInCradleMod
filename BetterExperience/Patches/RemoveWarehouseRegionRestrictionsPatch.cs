using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;

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
                try
                {
                    if (!ConfigManager.EnableAccessWarehouseAnywhere.Value)
                        return true;

                    __result = true;

                    HLog.Debug($"{nameof(RemoveWarehouseRegionRestrictionsPatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(RemoveWarehouseRegionRestrictionsPatch)}", ex);
                    return true;
                }
            }
        }
    }
}
