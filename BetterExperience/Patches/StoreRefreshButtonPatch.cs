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
        /// 商店页面内嵌"刷新商品"按钮。
        /// 原版商店库存只在日期更替、换图等事件时经惰性标记（need_summon_flush）刷新；
        /// 这里把刷新键追加进商店主界面命令列表（cmd_top：购买/出售/购物车/结算）的
        /// titles 数组，随原版 addButtonMultiT 一起建出第 5 个按钮——它是命令容器的
        /// 原生成员，键盘/手柄的上下导航（navi_auto_fill/navi_loop）与鼠标悬停全部
        /// 复用原版逻辑。点击后复刻酒吧打工商店（UiItemStoreBarBun）的原版重掷配方：
        /// 置 need_summon_flush |= MODE._ALL、need_reload_basic = true 后按"关-开"
        /// 顺序重新 InitManager，CreateItemStorage 应用挂起的 REMAKE（清空重读 .store、
        /// 重新随机 %CLIP_CATEGORY_KIND 商品池）；InitManager 开头的
        /// ReleaseStorage→abortCheckout 会安全回滚未结账购物车。免费无限制，仅作用
        /// 于当前商店。
        /// 注入点经 transpiler 把 titles 字段赋值（stfld）替换为注入方法调用，布局
        /// 不匹配时自动停用按钮，不影响原版逻辑。
        /// </summary>
        [HarmonyPatch]
        public static class StoreRefreshButtonPatch
        {
            internal const string RefreshButtonKey = "be_store_refresh";

            // STATE 是 protected 嵌套枚举，外部无法直接引用，经反射取值比较。
            private static readonly FieldInfo StateField = AccessTools.Field(typeof(UiItemStore), "stt");
            private static readonly FieldInfo ProductManagerField = AccessTools.Field(typeof(UiItemStore), "MngProduct");
            private static readonly MethodInfo ChangeStateMethod = AccessTools.Method(typeof(UiItemStore), "changeState");
            private static readonly object StateTop = StateField != null ? Enum.ToObject(StateField.FieldType, 0) : null;
            private static readonly object StateNoUse = StateField != null ? Enum.ToObject(StateField.FieldType, -1) : null;

            // transpiler 是否成功占位；失败时按钮整体停用，游戏原逻辑不受影响。
            private static bool _uiAvailable;

            [HarmonyCleanup]
            private static void Cleanup(Exception __exception)
            {
                if (__exception != null)
                {
                    _uiAvailable = false;
                    BLog.Error("Store refresh button disabled: Harmony registration failed.", __exception);
                }
            }

            // 把 titles 字段赋值替换为注入方法调用，刷新键随原版命令一起建按钮。
            [HarmonyTranspiler]
            [HarmonyPatch(typeof(UiItemStore), "createCommandWindow")]
            private static IEnumerable<CodeInstruction> CreateCommandWindowTranspiler(
                IEnumerable<CodeInstruction> instructions)
            {
                var original = instructions.ToList();
                _uiAvailable = TryBuildInjection(original, out var rewritten);
                if (!_uiAvailable)
                    BLog.Warn("Store refresh button disabled: createCommandWindow layout did not match the supported game version.");
                return rewritten;
            }

            internal static bool TryBuildInjection(
                IList<CodeInstruction> original,
                out List<CodeInstruction> rewritten)
            {
                rewritten = original.ToList();
                var titlesField = AccessTools.Field(typeof(DsnDataButtonMulti), "titles");
                var injectMethod = AccessTools.Method(typeof(StoreRefreshButtonPatch), nameof(InjectTitles));
                if (titlesField == null || injectMethod == null)
                    return false;

                int at = -1;
                for (int i = 0; i < original.Count; i++)
                {
                    if (original[i].opcode == OpCodes.Stfld
                        && original[i].operand is FieldInfo field && field == titlesField)
                    {
                        if (at >= 0)
                            return false;
                        at = i;
                    }
                }

                if (at < 0)
                    return false;

                // 栈上此时为 [实例, 数组]，与 stfld 完全等价地替换为同步消耗两值的调用。
                var call = new CodeInstruction(OpCodes.Call, injectMethod);
                call.labels.AddRange(original[at].labels);
                call.blocks.AddRange(original[at].blocks);
                rewritten[at] = call;
                return true;
            }

            private static void InjectTitles(DsnDataButtonMulti Mde, string[] titles)
            {
                Mde.titles = _uiAvailable && ConfigManager.EnableStoreRefresh?.Value == true
                    ? StoreRefreshButtonLogic.ExpandTitles(titles, RefreshButtonKey)
                    : titles;
            }

            // 按钮标题没有原版翻译条目，建出后用模组双语资源经皮肤覆盖显示文本；
            // 命令 tab 高度固定为 180（4 行 × 30 + 上下边距），第 5 行需要重算内容高度。
            [HarmonyPostfix]
            [HarmonyPatch(typeof(UiItemStore), "createCommandWindow")]
            private static void CreateCommandWindowPostfix(UiBoxDesigner ___BxC, Designer ___TabCTop)
            {
                try
                {
                    if (!_uiAvailable || ConfigManager.EnableStoreRefresh?.Value != true)
                        return;

                    var button = (___BxC.Get("cmd_top") as BtnContainerRunner)?.Get(RefreshButtonKey);
                    if (button == null)
                        throw new InvalidOperationException("The injected store refresh button was not found.");

                    button.get_Skin().setTitle(TranslatorResource.StoreRefreshButton.ToString());
                    ___TabCTop.cropBounds(-1f, ___TabCTop.get_sheight_px());
                    ___BxC.rowRemakeCheck(true);
                }
                catch (Exception ex)
                {
                    _uiAvailable = false;
                    BLog.Error("Store refresh button disabled for this screen.", ex);
                }
            }

            // 拦截刷新按钮的点击：执行重掷，未知标题仍走原版分支。
            [HarmonyPrefix]
            [HarmonyPatch(typeof(UiItemStore), "fnClickCmdTop")]
            private static bool CmdTopClickPrefix(UiItemStore __instance, aBtn B)
            {
                if (B == null || B.title != RefreshButtonKey)
                    return true;

                try
                {
                    RefreshStore(__instance, B);
                }
                catch (Exception ex)
                {
                    BLog.Error($"Unexpected error in {nameof(CmdTopClickPrefix)}.", ex);
                }
                return false;
            }

            // 点击刷新：按"退到 _NOUSE（内部安全回滚未结账购物车）→ 置重掷标记 → 重新
            // InitManager"的顺序执行，与原版事件重新打开商店（UiItemStoreBarBun）完全一致。
            private static void RefreshStore(UiItemStore ui, aBtn button)
            {
                if (button.isLocked() || StateField == null || ChangeStateMethod == null
                    || !Equals(StateField.GetValue(ui), StateTop))
                    return;

                var mng = ProductManagerField.GetValue(ui) as StoreManager;
                if (mng == null)
                    return;

                ChangeStateMethod.Invoke(ui, new[] { StateNoUse });
                mng.need_summon_flush |= StoreManager.MODE._ALL;
                mng.need_reload_basic = true;
                ui.InitManager(mng, ui.getUsingInventoryArray());

                SND.Ui.play("reset_var");
                NoticeGUI.Show(TranslatorResource.StoreRefreshed);
                BLog.Debug($"{nameof(StoreRefreshButtonPatch)} rerolled the stock of the current store.");
            }
        }
    }
}
