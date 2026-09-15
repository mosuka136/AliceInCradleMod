using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using UnityModBase.HClassAttribute;
using UnityModBase.HTranslatorSpace;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置当前酒店的食事餐券数量。
        /// 原版要求餐券与所属酒店绑定（foodticket_hotel），更换酒店会清空；
        /// 未绑定（从未购买含餐券的套餐）时无法安全新增，返回提示而不是写入。
        /// 新增按现有最高折扣生成（无餐券时 50%），扣减从折扣最低的尾部移除，
        /// 增减同步增减贵重品背包中的餐券物品，与原版 AddFoodTicket 的记账方式一致。
        /// </summary>
        [HarmonyPatch]
        public static class SetFoodTicketCountPatch
        {
            internal static readonly object NoticeOwner = new object();

            private static bool _initialized;

            [InitializeOnGameBoot]
            public static void Initialize()
            {
                if (_initialized)
                    return;

                _initialized = true;
                GameSaveLoadManager.OnGameSaveLoadCompleted += () =>
                {
                    try
                    {
                        if (ConfigManager.SetFoodTicketCount?.Value1 == true
                            && ConfigManager.SetFoodTicketCount.Value2 >= FoodTicketLogic.CountMin)
                            SetFoodTicketCount(ConfigManager.SetFoodTicketCount.Value2);
                    }
                    catch (Exception ex)
                    {
                        BLog.Error("Unexpected error in food ticket preload.", ex);
                    }
                };
            }

            /// <summary>
            /// 读取当前餐券数量；未进入游戏时返回 -1 占位。
            /// </summary>
            internal static int GetFoodTicketCount()
            {
                try
                {
                    if (GetM2D() == null)
                        return -1;

                    return GetTicketList()?.Count ?? -1;
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(GetFoodTicketCount)}.", ex);
                    return -1;
                }
            }

            internal static void SetFoodTicketCount(int value)
            {
                try
                {
                    if (value < FoodTicketLogic.CountMin)
                        return;

                    var m2d = GetM2D();
                    var imng = GetIMNG();
                    if (m2d == null || imng == null)
                    {
                        Notice(new Translator(chinese: "未进入游戏或尚未读档。", english: "Not in game or no save loaded."));
                        return;
                    }

                    if (string.IsNullOrEmpty(GetHotelBinding()))
                    {
                        Notice(new Translator(
                            chinese: "餐券尚未绑定酒店，请先在任一酒店购买一次含餐券的套餐。",
                            english: "Food tickets are not bound to a hotel yet; buy a ticket-included plan at any hotel first."
                            ));
                        return;
                    }

                    var ticket = NelItem.GetById(UiHotel.ticket_item_key);
                    if (ticket == null)
                        return;

                    var adiscount = GetTicketList();
                    if (adiscount == null)
                        return;

                    FoodTicketLogic.ComputeDelta(
                        adiscount.Count,
                        FoodTicketLogic.ClampCount(value),
                        out int add,
                        out int remove);

                    if (add > 0)
                    {
                        byte discount = FoodTicketLogic.NewTicketDiscount(adiscount);
                        int grade = imng.getInventoryPrecious()?.getInfo(ticket)?.top_grade ?? 0;
                        for (int i = 0; i < add; i++)
                        {
                            // 与原版 AddFoodTicket 相同的记账：物品入包成功才追加折扣字节。
                            if (imng.getItem(ticket, 1, grade) > 0)
                                adiscount.Add(discount);
                        }

                        adiscount.Sort((a, b) => (int)b - (int)a);
                    }

                    if (remove > 0)
                    {
                        adiscount.RemoveRange(adiscount.Count - remove, remove);
                        var info = imng.getInventoryPrecious()?.getInfo(ticket);
                        imng.getInventoryPrecious()?.Reduce(ticket, remove, info?.top_grade ?? 0);
                    }

                    BLog.Debug($"{nameof(SetFoodTicketCountPatch)} applied: {adiscount.Count}.");
                    Notice(new Translator(
                        chinese: $"餐券数量：{adiscount.Count}。",
                        english: $"Food tickets: {adiscount.Count}."
                        ));
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetFoodTicketCount)}.", ex);
                }
            }

            private static List<byte> GetTicketList()
            {
                return Traverse.Create(typeof(UiHotel)).Field("Afood_ticket").GetValue<List<byte>>();
            }

            private static string GetHotelBinding()
            {
                return Traverse.Create(typeof(UiHotel)).Field("foodticket_hotel").GetValue<string>();
            }

            private static void Notice(Translator message)
            {
                NoticeGUI.Show(message, owner: NoticeOwner);
            }
        }
    }
}
