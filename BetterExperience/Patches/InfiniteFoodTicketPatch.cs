using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 酒店用餐后不消耗食事餐券。
        /// 原版在 executeEat 成功时移除折扣列表首项并扣减一张贵重品餐券；
        /// 这里在原方法成功后把折扣与餐券原样归还，折扣优惠照常生效。
        /// 换酒店时原版会清空餐券，属既有语义，不在补正范围内。
        /// </summary>
        [HarmonyPatch]
        public static class InfiniteFoodTicketPatch
        {
            // executeEat 的前缀与后缀在同一线程背靠背执行，静态暂存即可传递捕获值。
            private static bool _captured;
            private static byte _capturedDiscount;
            private static int _capturedGrade;

            [HarmonyPrefix]
            [HarmonyPatch(typeof(UiLunchStoreHotel), "executeEat")]
            public static void ExecuteEatPrefix(UiLunchStoreHotel __instance)
            {
                _captured = false;
                try
                {
                    if (ConfigManager.EnableInfiniteFoodTicket?.Value != true)
                        return;

                    var adiscount = Traverse.Create(__instance).Field("Adiscount").GetValue<List<byte>>();
                    if (adiscount == null || adiscount.Count == 0)
                        return;

                    _capturedDiscount = adiscount[0];

                    var ticket = NelItem.GetById(UiLunchStoreHotel.ticket_item_key);
                    var info = GetIMNG()?.getInventoryPrecious()?.getInfo(ticket);
                    _capturedGrade = InfiniteFoodTicketLogic.RestoreGrade(info != null && info.total > 0, info?.top_grade ?? 0);

                    _captured = true;
                }
                catch (Exception ex)
                {
                    _captured = false;
                    BLog.Error($"Unexpected error in {nameof(ExecuteEatPrefix)}.", ex);
                }
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiLunchStoreHotel), "executeEat")]
            public static void ExecuteEatPostfix(UiLunchStoreHotel __instance, bool __result)
            {
                if (!__result || !_captured)
                    return;

                _captured = false;
                try
                {
                    var instanceTraverse = Traverse.Create(__instance);
                    InfiniteFoodTicketLogic.RestoreDiscount(
                        instanceTraverse.Field("Adiscount").GetValue<List<byte>>(),
                        _capturedDiscount);
                    instanceTraverse.Method("fineTicketDiscount").GetValue();

                    var ticket = NelItem.GetById(UiLunchStoreHotel.ticket_item_key);
                    var imng = GetIMNG();
                    if (ticket != null && imng != null)
                        imng.getItem(ticket, 1, _capturedGrade);

                    instanceTraverse.Field("ConHotel").GetValue<UiHotel>()?.fineFoodTicketBox();
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(ExecuteEatPostfix)}.", ex);
                }
            }
        }
    }
}
