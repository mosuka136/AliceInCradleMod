using System.Collections.Generic;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 酒店餐券数量的区间与增减规则，不访问游戏对象。
    /// 原版把每张餐券记为一个折扣字节（列表按折扣降序），
    /// 并要求等量的贵重品餐券物品与所属酒店绑定同时存在。
    /// </summary>
    internal static class FoodTicketLogic
    {
        internal const int CountMin = 0;
        internal const int CountMax = 99;
        /// <summary>无既有餐券时，新餐券使用的默认折扣百分比。</summary>
        internal const byte DefaultDiscount = 50;

        /// <summary>
        /// 目标数量约束到 0–99；负值表示不修改，由调用方先行判断。
        /// </summary>
        internal static int ClampCount(int count)
        {
            if (count < CountMin)
                return CountMin;

            if (count > CountMax)
                return CountMax;

            return count;
        }

        /// <summary>
        /// 新增餐券沿用现有最高折扣，没有餐券时使用默认折扣。
        /// </summary>
        internal static byte NewTicketDiscount(IReadOnlyList<byte> adiscount)
        {
            if (adiscount == null || adiscount.Count == 0)
                return DefaultDiscount;

            byte max = adiscount[0];
            for (int i = 1; i < adiscount.Count; i++)
            {
                if (adiscount[i] > max)
                    max = adiscount[i];
            }

            return max;
        }

        /// <summary>
        /// 计算从当前数量到目标数量需要增减的张数；增减不会同时发生。
        /// </summary>
        internal static void ComputeDelta(int current, int target, out int add, out int remove)
        {
            if (target > current)
            {
                add = target - current;
                remove = 0;
            }
            else
            {
                add = 0;
                remove = current - target;
            }
        }
    }
}
