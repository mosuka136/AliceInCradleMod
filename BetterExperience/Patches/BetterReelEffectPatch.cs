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
        [HarmonyPatch(typeof(ReelExecuter), "applyEffectToIK")]
        public class BetterReelEffectPatch
        {
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
                if (!ConfigManager.EnableBetterReelEffect.Value)
                    return;

                if (__instance == null)
                {
                    HLog.Error("__instance is null.");
                    return;
                }

                if (Reel == null || __instance == null)
                {
                    HLog.Error("Reel or __instance is null.");
                    return;
                }
                var acontent = Traverse.Create(Reel).Field("Acontent").GetValue<string[]>();
                if (acontent == null || __instance.IKRow == null ||
                    !FEnum<ReelExecuter.EFFECT>.TryParse(acontent[Reel.content_id_dec % acontent.Length], out var ik))
                {
                    HLog.Error("acontent is null or __instance.IKRow is null or cannot parse effect from acontent.");
                    return;
                }

                string[] sortedAcontent;
                switch (ik)
                {
                    case ReelExecuter.EFFECT.GRADE0:
                    case ReelExecuter.EFFECT.GRADE1:
                    case ReelExecuter.EFFECT.GRADE2:
                    case ReelExecuter.EFFECT.GRADE3:
                    case ReelExecuter.EFFECT.GRADE4:
                        sortedAcontent = SortByCustomOrder(acontent, _grade);
                        break;
                    case ReelExecuter.EFFECT.COUNT_ADD0:
                    case ReelExecuter.EFFECT.COUNT_ADD1:
                    case ReelExecuter.EFFECT.COUNT_ADD2:
                    case ReelExecuter.EFFECT.COUNT_ADD3:
                    case ReelExecuter.EFFECT.COUNT_ADD4:
                    case ReelExecuter.EFFECT.COUNT_ADD5:
                        sortedAcontent = SortByCustomOrder(acontent, _countAdd);
                        break;
                    case ReelExecuter.EFFECT.COUNT_MUL1:
                    case ReelExecuter.EFFECT.COUNT_MUL2:
                        sortedAcontent = SortByCustomOrder(acontent, _countMul);
                        break;
                    case ReelExecuter.EFFECT.ADD_MONEY10:
                    case ReelExecuter.EFFECT.ADD_MONEY20:
                    case ReelExecuter.EFFECT.ADD_MONEY30:
                    case ReelExecuter.EFFECT.ADD_MONEY100:
                        sortedAcontent = SortByCustomOrder(acontent, _addMoney);
                        break;
                    default:
                        sortedAcontent = null;
                        break;
                }

                if (sortedAcontent == null)
                {
                    HLog.Error($"No custom order defined for effect {ik}");
                    return;
                }
                var index = Array.IndexOf(acontent, sortedAcontent[0]);
                if (index < 0)
                {
                    HLog.Error($"Sorted acontent's first element '{sortedAcontent[0]}' not found in original acontent.");
                    return;
                }

                Reel.content_id_dec = index;
            }

            /// <summary>
            /// 按 customOrder 指定的顺序排序。
            /// 不在 customOrder 中的字符串放到最后，保持原顺序。
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
