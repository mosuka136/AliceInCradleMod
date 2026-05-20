using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using XX;

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
                    if (!ConfigManager.EnableBetterReelEffect.Value)
                        return;

                    if (Reel == null)
                    {
                        HLog.Notice("Reel is null.");
                        return;
                    }

                    var content = Traverse.Create(Reel).Field("Acontent").GetValue<string[]>();
                    if (content == null || __instance.IKRow == null || content.Length == 0 ||
                        !FEnum<ReelExecuter.EFFECT>.TryParse(content[Reel.content_id_dec % content.Length], out var ik))
                    {
                        HLog.Notice("content is null or __instance.IKRow is null or cannot parse effect from content.");
                        return;
                    }

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
                    {
                        HLog.Warn($"No custom order defined for effect {ik}");
                        return;
                    }
                    var index = Array.IndexOf(content, sortedContent[0]);
                    if (index < 0)
                    {
                        HLog.Warn($"Sorted content's first element '{sortedContent[0]}' not found in original content.");
                        return;
                    }

                    Reel.content_id_dec = index;
                    HLog.Debug($"{nameof(BetterReelEffectPatch)} applied.");
                }
                catch (Exception ex)
                {
                    HLog.Error($"Unexpected error in {nameof(BetterReelEffectPatch)}", ex);
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
                    .OrderBy(x => priority.TryGetValue(x.s, out var p) ? p : int.MaxValue)
                    .ThenBy(x => x.idx)
                    .Select(x => x.s)
                    .ToArray();
            }
        }
    }
}
