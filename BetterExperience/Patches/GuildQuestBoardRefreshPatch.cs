using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using HarmonyLib;
using nel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using UnityModBase.HTranslatorSpace;
using XX;

namespace BetterExperience.Patches
{
    public partial class HPatches
    {
        /// <summary>
        /// 公会任务板一键刷新（嵌入公会柜台界面）。
        /// 原版任务板内容只在日期类型变化或危险度累计满 9 时重掷；
        /// 这里在任务选择命令盒（UiGuildDepartBox）中追加一个"刷新任务板"按钮，
        /// 点击后移除所有未受领的任务条目并置重掷标记、调用 checkFineGQ 补板，
        /// 补板流程与原版日期变化完全一致（重置连续计数、按类别补满、随机奖励），
        /// 已受领任务与其任务卡不受影响；随后清空并重拷界面内各任务板的仓储、
        /// 原地重建列表。交付报告界面（digesting）不显示按钮。
        /// 按钮键经 transpiler 注入 Abtn_keys（Clear 之后、RemakeT 之前），
        /// 布局与导航全部复用原版命令网格；布局不匹配时自动停用按钮。
        /// </summary>
        [HarmonyPatch]
        public static class GuildQuestBoardRefreshPatch
        {
            internal const string RefreshButtonKey = "be_refresh_guild_board";

            private static readonly FieldInfo AbtnKeysField = AccessTools.Field(
                typeof(UiGuildDepartBox), "Abtn_keys");
            private static readonly MethodInfo ListClearMethod = AccessTools.Method(
                typeof(List<string>), nameof(List<string>.Clear));
            private static readonly MethodInfo InjectMethod = AccessTools.Method(
                typeof(GuildQuestBoardRefreshPatch), nameof(InjectRefreshKey));

            // transpiler 是否成功占位；失败时按钮整体停用，游戏原逻辑不受影响。
            private static bool _uiAvailable;
            // 选择任务瞬间的会话状态：配置开关与界面形态（交付报告界面不注入）。
            private static bool _showKey;

            [HarmonyCleanup]
            private static void Cleanup(Exception __exception)
            {
                if (__exception != null)
                {
                    _uiAvailable = false;
                    BLog.Error("Guild quest board refresh button disabled: Harmony registration failed.", __exception);
                }
            }

            // 任务选择时决定本次命令盒是否携带刷新按钮。
            [HarmonyPrefix]
            [HarmonyPatch(typeof(UiGQManageBox), "fineItemUsingCommand")]
            public static void FineUsingCommandPrefix(UiGQManageBox __instance)
            {
                try
                {
                    bool digesting = Traverse.Create(__instance).Field("digesting").GetValue<bool>();
                    _showKey = _uiAvailable
                        && ConfigManager.EnableGuildQuestBoardRefresh?.Value == true
                        && GuildQuestBoardRefreshLogic.ShowInGuildUi(digesting);
                }
                catch (Exception ex)
                {
                    _showKey = false;
                    BLog.Error($"Unexpected error in {nameof(FineUsingCommandPrefix)}.", ex);
                }
            }

            // 在 Abtn_keys.Clear() 之后注入刷新键，随原版 RemakeT 一起建按钮。
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(UiGuildDepartBox), nameof(UiGuildDepartBox.activateButtons))]
            public static IEnumerable<CodeInstruction> ActivateButtonsTranspiler(
                IEnumerable<CodeInstruction> instructions)
            {
                var original = instructions.ToList();
                _uiAvailable = TryBuildInjection(original, out var rewritten);
                if (!_uiAvailable)
                    BLog.Warn("Guild quest board refresh button disabled: activateButtons layout did not match the supported game version.");
                return rewritten;
            }

            internal static bool TryBuildInjection(
                IList<CodeInstruction> original,
                out List<CodeInstruction> rewritten)
            {
                rewritten = original.ToList();
                if (AbtnKeysField == null || ListClearMethod == null || InjectMethod == null)
                    return false;

                int at = -1;
                for (int i = 0; i + 1 < original.Count; i++)
                {
                    if (original[i].opcode == OpCodes.Ldfld
                        && original[i].operand is FieldInfo field && field == AbtnKeysField
                        && original[i + 1].opcode == OpCodes.Callvirt
                        && original[i + 1].operand is MethodInfo method && method == ListClearMethod)
                    {
                        if (at >= 0)
                            return false;
                        at = i + 2;
                    }
                }

                if (at < 0 || at > original.Count)
                    return false;

                rewritten.InsertRange(at, new[]
                {
                    new CodeInstruction(OpCodes.Ldarg_0),
                    new CodeInstruction(OpCodes.Call, InjectMethod)
                });
                return true;
            }

            private static void InjectRefreshKey(UiGuildDepartBox box)
            {
                if (!_uiAvailable || !_showKey || box == null)
                    return;

                try
                {
                    (AbtnKeysField.GetValue(box) as List<string>)?.Add(RefreshButtonKey);
                }
                catch (Exception ex)
                {
                    _uiAvailable = false;
                    BLog.Error("Guild quest board refresh button disabled for this screen.", ex);
                }
            }

            // 按钮随 RemakeT 重建，标题需要每次用模组双语资源经皮肤 setTitle 覆盖。
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiGuildDepartBox), nameof(UiGuildDepartBox.activateButtons))]
            public static void ActivateButtonsPostfix(UiGuildDepartBox __instance)
            {
                try
                {
                    if (!_uiAvailable || !_showKey)
                        return;

                    var container = Traverse.Create(__instance).Field("BConRB")
                        .GetValue() as BtnContainerRadio<aBtn>;
                    var button = container?.Get(RefreshButtonKey);
                    button?.get_Skin().setTitle(TranslatorResource.GuildBoardRefresh.ToString());
                }
                catch (Exception ex)
                {
                    _uiAvailable = false;
                    BLog.Error($"Unexpected error in {nameof(ActivateButtonsPostfix)}.", ex);
                }
            }

            // 拦截刷新按钮的点击：执行重掷并原地重建柜台列表，未知标题仍走原版分支。
            [HarmonyPrefix]
            [HarmonyPatch(typeof(UiGQManageBox), "fnClickDepertCmd")]
            public static bool DepartCmdPrefix(UiGQManageBox __instance, aBtn B)
            {
                if (B == null || B.title != RefreshButtonKey)
                    return true;

                try
                {
                    RefreshFromGuildUi(__instance);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(DepartCmdPrefix)}.", ex);
                }
                return false;
            }

            private static void RefreshFromGuildUi(UiGQManageBox box)
            {
                var traverse = Traverse.Create(box);
                var guild = traverse.Field("M2D").GetValue<NelM2DBase>()?.GUILD;
                int removed = RerollUnaccepted(guild);
                if (removed < 0)
                    return;

                SND.Ui.play("reset_var");
                // 与原版 Cancel 同路径退出任务选择态（收起命令盒、恢复列表与页签输入）。
                box.changeStateToSelect();
                RebuildBoardStorages(traverse);
                BLog.Debug($"{nameof(GuildQuestBoardRefreshPatch)} rerolled {removed} entries from the guild UI.");
            }

            /// <summary>
            /// 重掷所有任务板上未受领的任务条目；返回移除数量，任务板未初始化时返回 -1。
            /// </summary>
            internal static int RerollUnaccepted(GuildManager guild)
            {
                if (guild == null)
                    return -1;

                var traverse = Traverse.Create(guild);
                // 补板需要有效的任务类型；读档后该字段为空，回退到最近一次有效值。
                string enableType = traverse.Field("current_gq_enable_type").GetValue<string>();
                if (string.IsNullOrEmpty(enableType))
                    enableType = traverse.Field("gq_valid_type").GetValue<string>();
                if (string.IsNullOrEmpty(enableType))
                    return -1;

                var lists = new List<GuildManager.GQEntryList>();
                foreach (var kvp in guild.getCurrentEntryWholeObject())
                    lists.Add(kvp.Value);

                int removed = 0;
                foreach (var list in lists)
                {
                    for (int i = list.Count - 1; i >= 0; i--)
                    {
                        var entry = list[i];
                        if (!GuildQuestBoardRefreshLogic.IsRemovable(
                                entry.Qt != null, entry.destructed))
                            continue;

                        guild.removeQuest(entry);
                        removed++;
                    }
                }

                traverse.Field("need_fine_flag_").SetValue(
                    (byte)(traverse.Field("need_fine_flag_").GetValue<byte>() | 1));
                guild.checkFineGQ(enableType);
                return removed;
            }

            private static void RebuildBoardStorages(Traverse boxTraverse)
            {
                // 界面仓储是 createUi 时的一次性拷贝；重掷后清空重拷并重建当前页行，
                // 其余页签在切换时经原版 fnItemTabChanged → initItemStorage 重建。
                var storages = boxTraverse.Field("AStorage").GetValue<List<ItemStorage>>();
                var lists = boxTraverse.Field("AAEntry").GetValue<List<GuildManager.GQEntryList>>();
                int pairs = Math.Min(storages?.Count ?? 0, lists?.Count ?? 0);
                for (int i = 0; i < pairs; i++)
                {
                    storages[i].clearAllItems(storages[i].row_max);
                    lists[i].copyRowsToStorage("", storages[i]);
                }

                boxTraverse.Method("initItemStorage").GetValue();
            }
        }
    }
}
