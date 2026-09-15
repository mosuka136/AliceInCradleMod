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
        /// 按倍率调整商店的购买与出售单价。
        /// 原版商店的界面显示与结算金额都经由 buyPrice/sellPrice 计算，
        /// 在两处方法返回值上统一应用倍率即可同时覆盖显示与实际扣款/收款；
        /// 不改写商店实例的 ratio 字段，避免影响存档序列化的商店状态。
        /// </summary>
        [HarmonyPatch]
        public static class StorePricePatch
        {
            [HarmonyPostfix]
            [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.buyPrice))]
            public static void BuyPricePostfix(ref int __result)
            {
                try
                {
                    if (ConfigManager.EnableStorePriceRatio?.Value != true)
                        return;

                    float ratio = StorePriceLogic.ClampRatio(
                        ConfigManager.SetBuyPriceRatio?.Value ?? StorePriceLogic.RatioDefault,
                        StorePriceLogic.BuyRatioMax);
                    __result = StorePriceLogic.ApplyRatio(__result, ratio);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BuyPricePostfix)}.", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(StoreManager), nameof(StoreManager.sellPrice))]
            public static void SellPricePostfix(ref int __result)
            {
                try
                {
                    if (ConfigManager.EnableStorePriceRatio?.Value != true)
                        return;

                    float ratio = StorePriceLogic.ClampRatio(
                        ConfigManager.SetSellPriceRatio?.Value ?? StorePriceLogic.RatioDefault,
                        StorePriceLogic.SellRatioMax);
                    __result = StorePriceLogic.ApplyRatio(__result, ratio);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SellPricePostfix)}.", ex);
                }
            }
        }
    }
}
