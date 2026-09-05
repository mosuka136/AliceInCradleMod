using BetterExperience.BConfigManager;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 锁定任务与购物产生的积分变更，不保存跨存档的锁定快照。
        /// 原动画仍执行结束清理，商店仍执行商品交付及购买资格检查。
        /// </summary>
        [HarmonyPatch]
        public static class LockGuildPointPatch
        {
            private static readonly FieldInfo PointBoxMapField = typeof(FillBlockGQPointBox).GetField("M2D", BindingFlags.Instance | BindingFlags.NonPublic);
            private static bool IsLocked => ConfigManager.EnableLockGuildPoint?.Value == true;

            internal static void WriteAnimatedPoint(GuildManager guild, int point)
            {
                WriteAnimatedPoint(guild, point, IsLocked);
            }

            internal static void WriteAnimatedPoint(GuildManager guild, int point, bool locked)
            {
                if (guild != null && (!locked || guild.gq_point < 0))
                    guild.gq_point = point;
            }

            [HarmonyPrefix]
            [HarmonyPatch(typeof(FillImageBlockExperience), "SetObtaining")]
            public static void SetObtainingPrefix(FillImageBlockExperience __instance, ref int val)
            {
                if (__instance is FillBlockGQPointBox && IsLocked)
                    val = 0;
            }

            // 每次动画推进都采用真实积分，包括动画中途开启锁定的情况。
            // 不跳过原方法，保证等待动画的任务/商店流程能够正常结束。
            [HarmonyPrefix]
            [HarmonyPatch(typeof(FillImageBlockExperience), "runAnimating")]
            public static void RunAnimatingPrefix(FillImageBlockExperience __instance, ref float ___cur_pt_anm_first)
            {
                if (!(__instance is FillBlockGQPointBox) || !IsLocked)
                    return;

                var guild = ((NelM2DBase)PointBoxMapField.GetValue(__instance))?.GUILD;
                SynchronizeAnimation(__instance, guild, ref ___cur_pt_anm_first, true);
            }

            internal static void SynchronizeAnimation(FillImageBlockExperience block, GuildManager guild, ref float initialPoint, bool locked)
            {
                if (!locked || !(block is FillBlockGQPointBox) || guild == null || guild.gq_point < 0)
                    return;

                int rank = Math.Min(3, guild.current_grank);
                block.initMeter(rank, guild.gq_point, guild.grank_start_point(rank), guild.grank_start_point(rank + 1));
                initialPoint = guild.gq_point;
                block.stack_obtained = 0;
                block.stack_obtaining = 0;
            }

            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiItemStoreGRank), "confirmCheckout")]
            public static void CheckoutPostfix(NelM2DBase ___M2D, CoinEntry ___CEntry)
            {
                SynchronizeShopBalance(___M2D?.GUILD, ___CEntry, IsLocked);
            }

            internal static void SynchronizeShopBalance(GuildManager guild, CoinEntry entry, bool locked)
            {
                if (locked && guild != null && guild.gq_point >= 0 && entry != null)
                    entry.Set((uint)guild.gq_point, true);
            }

            [HarmonyPatch]
            public static class PointWritePatch
            {
                [HarmonyTargetMethods]
                public static IEnumerable<MethodBase> TargetMethods()
                {
                    yield return typeof(FillBlockGQPointBox).GetMethod("animationFinished", BindingFlags.Instance | BindingFlags.NonPublic);
                    yield return typeof(FillBlockGQPointBox).GetMethod("triggerLevelChangedInAnimation", BindingFlags.Instance | BindingFlags.NonPublic);
                }

                // 仅替换两个积分动画方法中的字段写入，保留基类清理及其余控制流。
                // 初始化和存档读取的同名字段写入不在补丁范围内。
                [HarmonyTranspiler]
                public static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
                {
                    var result = instructions.Select(instruction => new CodeInstruction(instruction)).ToList();
                    var field = typeof(GuildManager).GetField(nameof(GuildManager.gq_point));
                    var setter = typeof(LockGuildPointPatch).GetMethod(nameof(WriteAnimatedPoint), BindingFlags.Static | BindingFlags.NonPublic,
                        null, new[] { typeof(GuildManager), typeof(int) }, null);
                    int replaced = 0;
                    foreach (var instruction in result)
                    {
                        if (instruction.opcode != OpCodes.Stfld || !Equals(instruction.operand, field))
                            continue;

                        instruction.opcode = OpCodes.Call;
                        instruction.operand = setter;
                        replaced++;
                    }
                    if (replaced != 1)
                        throw new InvalidOperationException($"Expected one guild point write, found {replaced}.");
                    return result;
                }
            }
        }
    }
}
