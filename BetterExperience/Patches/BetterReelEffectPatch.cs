using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 在转轮效果生效前选择同类效果中更优先的结果。
        /// 补丁通过重写 <c>content_id_dec</c> 指向排序后的首个效果，不修改转轮内容数组本身。
        /// </summary>
        [HarmonyPatch(typeof(ReelExecuter), "applyEffectToIK")]
        public class BetterReelEffectPatch
        {
            private static bool _missingSpecifiedEffectLogged;
            // 每个数组按“优先采用”的顺序排列；未列入的效果保留原顺序并排在后面。
            private readonly static string[] _grade = new string[]
                {
                    ReelExecuter.EFFECT.GRADE4.ToString(),
                    ReelExecuter.EFFECT.GRADE3.ToString(),
                    ReelExecuter.EFFECT.GRADE2.ToString(),
                    ReelExecuter.EFFECT.GRADE1.ToString(),
                    ReelExecuter.EFFECT.GRADE0.ToString()
                };
            private readonly static string[] _countAdd = new string[]
                {
                    ReelExecuter.EFFECT.COUNT_ADD5.ToString(),
                    ReelExecuter.EFFECT.COUNT_ADD4.ToString(),
                    ReelExecuter.EFFECT.COUNT_ADD3.ToString(),
                    ReelExecuter.EFFECT.COUNT_ADD2.ToString(),
                    ReelExecuter.EFFECT.COUNT_ADD1.ToString(),
                    ReelExecuter.EFFECT.COUNT_ADD0.ToString()
                };
            private readonly static string[] _countMul = new string[]
                {
                    ReelExecuter.EFFECT.COUNT_MUL2.ToString(),
                    ReelExecuter.EFFECT.COUNT_MUL1.ToString()
                };
            private readonly static string[] _addMoney = new string[]
                {
                    ReelExecuter.EFFECT.ADD_MONEY100.ToString(),
                    ReelExecuter.EFFECT.ADD_MONEY30.ToString(),
                    ReelExecuter.EFFECT.ADD_MONEY20.ToString(),
                    ReelExecuter.EFFECT.ADD_MONEY10.ToString(),
                };

            public static void Prefix(ReelExecuter __instance, ReelExecuter Reel)
            {
                try
                {
                    if (ConfigManager.EnableBetterExperience?.Value != true)
                        return;

                    bool betterEffect = ConfigManager.EnableBetterReelEffect?.Value == true;
                    var specified = ConfigManager.SpecifiedLuckyBagEffect?.Value ?? LuckyBagEffect.Default;
                    if (!betterEffect && specified == LuckyBagEffect.Default)
                        return;

                    if (Reel == null || __instance?.IKRow == null || Reel.content_id_dec < 0)
                        return;

                    var content = Traverse.Create(Reel).Field("Acontent").GetValue<string[]>();
                    if (TrySelectEffect(content, Reel.content_id_dec, Reel.getEType(), specified,
                        betterEffect, out int index, out bool missingSpecifiedEffect))
                        Reel.content_id_dec = index;

                    if (missingSpecifiedEffect && !_missingSpecifiedEffectLogged)
                    {
                        _missingSpecifiedEffectLogged = true;
                        BLog.Warn("Specified Lucky Bag effect is absent; keeping this reel's original result.");
                    }
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(BetterReelEffectPatch)}", ex);
                }
            }

            /// <summary>
            /// 只选择索引，不改写共享数组。指定效果缺失时也不回退到自动择优。
            /// </summary>
            internal static bool TrySelectEffect(string[] content, int currentIndex, ReelExecuter.ETYPE type,
                LuckyBagEffect specified, bool betterEffect, out int index, out bool missingSpecifiedEffect)
            {
                index = currentIndex;
                missingSpecifiedEffect = false;
                if (content == null || content.Length == 0 || currentIndex < 0)
                    return false;

                string requested = GetSpecifiedEffect(specified);
                if (type == ReelExecuter.ETYPE.RANDOM && requested != null)
                {
                    int selected = Array.FindIndex(content, value =>
                        string.Equals(value, requested, StringComparison.OrdinalIgnoreCase));
                    if (selected < 0)
                    {
                        missingSpecifiedEffect = true;
                        return false;
                    }
                    index = selected;
                    return true;
                }

                if (!betterEffect || !Enum.TryParse(content[currentIndex % content.Length], true,
                    out ReelExecuter.EFFECT ik))
                    return false;

                string[] sortedContent;
                switch (ik)
                {
                    case ReelExecuter.EFFECT.GRADE0:
                    case ReelExecuter.EFFECT.GRADE1:
                    case ReelExecuter.EFFECT.GRADE2:
                    case ReelExecuter.EFFECT.GRADE3:
                    case ReelExecuter.EFFECT.GRADE4:
                        sortedContent = SortByCustomOrder(content, _grade);
                        break;
                    case ReelExecuter.EFFECT.COUNT_ADD0:
                    case ReelExecuter.EFFECT.COUNT_ADD1:
                    case ReelExecuter.EFFECT.COUNT_ADD2:
                    case ReelExecuter.EFFECT.COUNT_ADD3:
                    case ReelExecuter.EFFECT.COUNT_ADD4:
                    case ReelExecuter.EFFECT.COUNT_ADD5:
                        sortedContent = SortByCustomOrder(content, _countAdd);
                        break;
                    case ReelExecuter.EFFECT.COUNT_MUL1:
                    case ReelExecuter.EFFECT.COUNT_MUL2:
                        sortedContent = SortByCustomOrder(content, _countMul);
                        break;
                    case ReelExecuter.EFFECT.ADD_MONEY10:
                    case ReelExecuter.EFFECT.ADD_MONEY20:
                    case ReelExecuter.EFFECT.ADD_MONEY30:
                    case ReelExecuter.EFFECT.ADD_MONEY100:
                        sortedContent = SortByCustomOrder(content, _addMoney);
                        break;
                    default:
                        sortedContent = null;
                        break;
                }

                if (sortedContent == null)
                    return false;

                index = Array.IndexOf(content, sortedContent[0]);
                return index >= 0;
            }

            private static string GetSpecifiedEffect(LuckyBagEffect effect)
            {
                switch (effect)
                {
                    case LuckyBagEffect.CountAdd1: return "COUNT_ADD1";
                    case LuckyBagEffect.CountAdd2: return "COUNT_ADD2";
                    case LuckyBagEffect.CountAdd3: return "COUNT_ADD3";
                    case LuckyBagEffect.GradeAdd1: return "GRADE1";
                    case LuckyBagEffect.GradeAdd2: return "GRADE2";
                    case LuckyBagEffect.GradeAdd3: return "GRADE3";
                    case LuckyBagEffect.CountMultiply2: return "COUNT_MUL2";
                    case LuckyBagEffect.MoneyAdd100: return "ADD_MONEY100";
                    default: return null;
                }
            }

            /// <summary>
            /// 按指定优先级对转轮效果文本排序。
            /// 不在自定义顺序中的值会放到最后，并保持它们在原数组中的相对顺序。
            /// </summary>
            public static string[] SortByCustomOrder(
                string[] input,
                string[] customOrder,
                StringComparer comparer = null)
            {
                if (input == null || customOrder == null)
                    throw new ArgumentNullException();
                if (comparer == null)
                    comparer = StringComparer.OrdinalIgnoreCase;

                var priority = new Dictionary<string, int>(comparer);
                for (int i = 0; i < customOrder.Length; i++)
                {
                    // 若 customOrder 有重复项，只取第一次出现的优先级
                    if (!priority.ContainsKey(customOrder[i]))
                        priority[customOrder[i]] = i;
                }

                return input
                    .Select((s, idx) => new { s, idx })
                    .OrderBy(x => x.s != null && priority.TryGetValue(x.s, out var p) ? p : int.MaxValue)
                    .ThenBy(x => x.idx)
                    .Select(x => x.s)
                    .ToArray();
            }

            public enum LuckyBagEffect
            {
                [Description("保持原行为 / Default")]
                Default,
                [Description("数量 +1 / Amount +1")]
                CountAdd1,
                [Description("数量 +2 / Amount +2")]
                CountAdd2,
                [Description("数量 +3 / Amount +3")]
                CountAdd3,
                [Description("品质 +1 / Grade +1")]
                GradeAdd1,
                [Description("品质 +2 / Grade +2")]
                GradeAdd2,
                [Description("品质 +3 / Grade +3")]
                GradeAdd3,
                [Description("数量 ×2 / Amount ×2")]
                CountMultiply2,
                [Description("金币 +100 / Gold +100")]
                MoneyAdd100
            }
        }
    }
}
