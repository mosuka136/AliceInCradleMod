using BetterExperience.BConfigManager;
using BetterExperience.HLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 放宽访问仓库库存的区域限制。
        /// 配置开启时直接允许访问房屋仓库库存，调用方会继续执行后续 UI 流程。
        /// </summary>
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
