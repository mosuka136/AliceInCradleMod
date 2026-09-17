using BetterExperience.BLogSpace;
using nel;
using System;
using System.ComponentModel;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 设置玩家怀卵的数量与种类。种类选择保存在本类；数量条目实时读取所选种类的
        /// 实际怀卵数，写入时先整类移除再按目标数量重新添加（PrEggManager.Remove/Add），
        /// 结果精确且 UI、状态计数由游戏自身刷新。
        /// </summary>
        internal static class PrEgg
        {
            internal enum EggCateg
            {
                [Description("幼虫")] Worm = 0,
                [Description("史莱姆")] Slime = 1,
                [Description("蘑菇")] Mush = 2,
                [Description("巨人的精液")] GolemOd = 3,
                [Description("野猪")] Pig = 4,
                [Description("妖狐的精液")] Fox = 5,
                [Description("山蜘蛛")] BossSpider = 6
            }

            internal const int CountMin = 0;
            internal const int CountMax = 99;

            private static EggCateg _categ = EggCateg.Worm;

            internal static EggCateg GetEggCategory() => _categ;

            internal static void SetEggCategory(EggCateg value)
            {
                try
                {
                    _categ = value;
                    BLog.Debug($"Egg category selected: {value}.");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetEggCategory)}.", ex);
                }
            }

            /// <summary>未进游戏或玩家尚未加载时返回 -1。</summary>
            internal static int GetEggCount()
            {
                var eggCon = GetPR()?.EggCon;
                if (eggCon == null)
                    return -1;

                var item = eggCon.Get(ToGameCateg(_categ));
                return item == null ? 0 : (int)item.val;
            }

            internal static void SetEggCount(int count)
            {
                try
                {
                    if (count < CountMin || count > CountMax)
                    {
                        BLog.Notice($"Ignored invalid egg count: {count}");
                        return;
                    }

                    var eggCon = GetPR()?.EggCon;
                    if (eggCon == null)
                    {
                        BLog.Notice("Player egg manager not found while applying egg count.");
                        return;
                    }

                    var categ = ToGameCateg(_categ);
                    eggCon.Remove(categ);
                    if (count > 0)
                        eggCon.Add(categ, count);
                    BLog.Debug($"Egg count applied. Categ: {categ}, count: {count}.");
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(SetEggCount)}.", ex);
                }
            }

            private static PrEggManager.CATEG ToGameCateg(EggCateg value)
            {
                switch (value)
                {
                    case EggCateg.Worm: return PrEggManager.CATEG.WORM;
                    case EggCateg.Slime: return PrEggManager.CATEG.SLIME;
                    case EggCateg.Mush: return PrEggManager.CATEG.MUSH;
                    case EggCateg.GolemOd: return PrEggManager.CATEG.GOLEM_OD;
                    case EggCateg.Pig: return PrEggManager.CATEG.PIG;
                    case EggCateg.Fox: return PrEggManager.CATEG.FOX;
                    case EggCateg.BossSpider: return PrEggManager.CATEG.BOSS_SPIDER;
                    default: return PrEggManager.CATEG._OFFLINE;
                }
            }
        }
    }
}
