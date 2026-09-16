using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 战斗结束后食物不腐败。
        /// 原版在非安全区结束战斗（含战败）时把背包与掉落物中的生食料理替换为腐败食物，
        /// 并清空咖啡机餐券、解除酒店餐券绑定；这里在开启配置后跳过整个腐败流程。
        /// </summary>
        [HarmonyPatch]
        public static class NoFoodSpoilagePatch
        {
            [HarmonyPrefix]
            [HarmonyPatch(typeof(NelItemManager), nameof(NelItemManager.spoilRawDishes))]
            public static bool SpoilRawDishesPrefix()
            {
                try
                {
                    if (ConfigManager.EnableNoFoodSpoilage?.Value != true)
                        return true;

                    BLog.Debug($"{nameof(NoFoodSpoilagePatch)} applied.");
                    return false;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SpoilRawDishesPrefix)}.", ex);
                    return true;
                }
            }
        }
    }
}
