using System.Collections.Generic;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 酒店餐券不消耗的判定与归还规则，不访问游戏对象。
    /// 原版在用餐成功后移除折扣列表的首个元素并扣减一张贵重品餐券；
    /// 归还时把被消耗的折扣插回列表头部（列表按折扣降序，移除的总是最大值）。
    /// </summary>
    internal static class InfiniteFoodTicketLogic
    {
        /// <summary>
        /// 用餐成功且餐前持有餐券时才需要归还。
        /// </summary>
        internal static bool ShouldRestore(bool featureEnabled, bool hadTicketsBeforeEat, bool eatSucceeded)
        {
            return featureEnabled && hadTicketsBeforeEat && eatSucceeded;
        }

        /// <summary>
        /// 把被消耗的折扣字节插回列表头部，保持降序不变。
        /// </summary>
        internal static void RestoreDiscount(List<byte> adiscount, byte consumed)
        {
            if (adiscount == null)
                return;

            adiscount.Insert(0, consumed);
        }

        /// <summary>
        /// 归还餐券使用的品级：仍能查到持有信息时用最高品级，否则回退 0。
        /// </summary>
        internal static int RestoreGrade(bool hasTicketInfo, int topGrade)
        {
            return hasTicketInfo ? topGrade : 0;
        }
    }
}
