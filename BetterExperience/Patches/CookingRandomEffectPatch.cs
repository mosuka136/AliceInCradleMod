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
using System.Runtime.CompilerServices;
using XX;

namespace BetterExperience.Patches
{
    /// <summary>
    /// 将随机效果预览和按钮接入原生料理界面；库存扣除、成品注册和批量制作仍由游戏执行。
    /// 所有回调均在游戏主线程运行，配置按 InitManager 开始的界面会话快照。
    /// </summary>
    [HarmonyPatch]
    internal static class CookingRandomEffectPatch
    {
        private const int RecipeTopic = 0;
        private const int RecipeChooseRow = 1;
        private const int RecipeConfirm = 5;
        private const int Complete = 6;
        private const string ButtonName = "be_cooking_reroll";
        internal const float ButtonRowHeight = 40f;
        private const BindingFlags InstanceMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

        private static readonly ConditionalWeakTable<UiCraftBase, Session> Sessions = new ConditionalWeakTable<UiCraftBase, Session>();
        private static readonly FieldInfo DishField = typeof(UiCraftBase).GetField("CompletionImage", InstanceMembers);
        private static readonly FieldInfo IngredientsField = typeof(UiCraftBase).GetField("AAIngCreate", InstanceMembers);
        private static readonly FieldInfo ReadOnlyField = typeof(UiCraftBase).GetField("read_only", InstanceMembers);
        private static readonly FieldInfo StateField = typeof(UiCraftBase).GetField("stt", InstanceMembers);
        private static readonly MethodInfo RefreshMethod = typeof(UiCraftBase).GetMethod("fineCompletionDetail", InstanceMembers);
        private static bool _available;

        private sealed class Session
        {
            internal bool Enabled;
            internal readonly CookingEffectPreview Preview = new CookingEffectPreview();
            internal CookingEffectCommit Commit;
            internal aBtn RerollButton;
        }

        [HarmonyCleanup]
        private static void Cleanup(Exception __exception)
        {
            if (__exception != null)
            {
                _available = false;
                BLog.Error("Cooking random effect preview disabled: Harmony registration failed.", __exception);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiCraftBase), nameof(UiCraftBase.InitManager))]
        private static void InitializePostfix(UiCraftBase __instance)
        {
            Sessions.Remove(__instance);
            Sessions.Add(__instance, new Session
            {
                Enabled = _available && ConfigManager.EnableCookingRandomEffectPreview?.Value == true
            });
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UiCraftBase), "changeState")]
        private static void ChangeStatePrefix(UiCraftBase __instance, int __0)
        {
            if (Sessions.TryGetValue(__instance, out var session) && (__0 <= RecipeTopic || __0 >= Complete))
                session.Preview.Clear();
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UiCraftBase), nameof(UiCraftBase.OnDestroy))]
        private static void DestroyPrefix(UiCraftBase __instance)
        {
            Sessions.Remove(__instance);
        }

        private static bool TryGetPreview(UiCraftBase ui, out Session session, out RCP.RecipeDish dish)
        {
            dish = null;
            if (!Sessions.TryGetValue(ui, out session) || !_available || !session.Enabled)
                return false;
            int state = Convert.ToInt32(StateField.GetValue(ui));
            if (state < RecipeChooseRow || state > RecipeConfirm)
                return false;
            dish = (RCP.RecipeDish)DishField.GetValue(ui);
            bool eligible = CookingEffectPreview.IsEligible(true, (bool)ReadOnlyField.GetValue(ui), dish);
            if (!eligible)
                session.Preview.Clear();
            return eligible;
        }

        private static List<List<UiCraftBase.IngEntryRow>> GetIngredients(UiCraftBase ui)
        {
            return (List<List<UiCraftBase.IngEntryRow>>)IngredientsField.GetValue(ui);
        }

        private static void Refresh(UiCraftBase ui)
        {
            using (var text = TX.PopBld())
                RefreshMethod.Invoke(ui, new object[] { text, true });
        }

        private static void Disable(UiCraftBase ui, Exception exception, bool refresh = false)
        {
            if (Sessions.TryGetValue(ui, out var session))
            {
                session.Enabled = false;
                session.Preview.Clear();
                if (session.RerollButton != null)
                    session.RerollButton.SetLocked(true);
            }
            BLog.Error("Cooking random effect preview disabled for this screen.", exception);
            if (refresh)
            {
                try
                {
                    Refresh(ui);
                }
                catch (Exception ex)
                {
                    BLog.Error("Unable to refresh native cooking details after disabling the preview.", ex);
                }
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UiCraftBase), "getCompletionDetail")]
        private static bool DetailPrefix(UiCraftBase __instance, STB Stb)
        {
            try
            {
                if (!TryGetPreview(__instance, out var session, out var dish))
                    return true;
                session.Preview.Update(dish, GetIngredients(__instance));

                // 先在临时文本中完成渲染；失败时原版仍能从未修改的 CompletionImage 正常显示。
                using (var text = TX.PopBld())
                using (var value = TX.PopBld())
                {
                    text.AddTxA("Item_for_food_cost").TxRpl(RCP.getCostStringTo(value, dish.cost));
                    var fixedEffects = session.Preview.GetEffects(false);
                    if (fixedEffects.OEffect.Count > 0)
                    {
                        text.Add("\n\n");
                        RCP.getEffectListupTo(text, fixedEffects, 1f, "\n");
                    }
                    text.Add("\n\n").Add(TranslatorResource.CookingRandomPreview.ToString()).Add("\n");
                    RCP.getEffectListupTo(text, session.Preview.GetEffects(true), 1f, "\n");
                    Stb.Add(text);
                }
                session.Preview.Displayed = true;
                return false;
            }
            catch (Exception ex)
            {
                Disable(__instance, ex);
                return true;
            }
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(UiCraftBase), "initCmd")]
        private static IEnumerable<CodeInstruction> LayoutTranspiler(IEnumerable<CodeInstruction> instructions)
        {
            var original = instructions.ToList();
            _available = TryReserveButtonRow(original, out var rewritten);
            if (!_available)
                BLog.Warn("Cooking random effect preview disabled: initCmd layout did not match the supported game version.");
            return rewritten;
        }

        internal static bool TryReserveButtonRow(IList<CodeInstruction> original, out List<CodeInstruction> rewritten)
        {
            rewritten = original.ToList();
            var offsets = Enumerable.Range(0, original.Count)
                .Where(i => original[i].opcode == OpCodes.Ldc_R4 && Equals(original[i].operand, -60f)).ToArray();
            if (offsets.Length != 1)
                return false;
            int at = offsets[0];
            // 同时验证确认分支、底部 Tab 和原生效果区入口，避免仅凭一个浮点常量修改其他逻辑。
            bool hasState = original.Take(at).Any(i => i.opcode == OpCodes.Ldfld && Equals(i.operand, StateField));
            bool hasBottom = original.Skip(at).Any(i => i.opcode == OpCodes.Ldstr && Equals(i.operand, "Cmd-bottom"));
            bool hasImage = original.Skip(at).Any(i => i.operand is MethodInfo method && method.Name == "createCmdFIB" && method.DeclaringType == typeof(UiCraftBase));
            if (!hasState || !hasBottom || !hasImage || at + 1 >= original.Count || !original[at + 1].opcode.Name.StartsWith("stloc", StringComparison.Ordinal))
                return false;
            rewritten.InsertRange(at + 1, new[]
            {
                new CodeInstruction(OpCodes.Ldarg_0),
                new CodeInstruction(OpCodes.Call, typeof(CookingRandomEffectPatch).GetMethod(nameof(AdjustConfirmOffset), BindingFlags.Static | BindingFlags.NonPublic))
            });
            return true;
        }

        private static float AdjustConfirmOffset(float original, UiCraftBase ui)
        {
            try
            {
                return TryGetPreview(ui, out _, out _) ? original - ButtonRowHeight : original;
            }
            catch (Exception ex)
            {
                Disable(ui, ex);
                return original;
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UiCraftBase), "initCmd")]
        private static void CommandPostfix(UiCraftBase __instance, UiBoxDesigner ___BxCmd)
        {
            var command = ___BxCmd;
            bool tabSelected = false;
            try
            {
                if (!TryGetPreview(__instance, out var session, out _) || Convert.ToInt32(StateField.GetValue(__instance)) != RecipeConfirm)
                    return;
                var bottom = command.getTab("Cmd-bottom")
                    ?? throw new InvalidOperationException("The native cooking command tab was not found.");
                var submit = bottom.getBtn("&&Submit_alchemy")
                    ?? throw new InvalidOperationException("The native cooking submit button was not found.");
                var selected = aBtn.PreSelected;
                var buttons = bottom.getBtnContainer();
                int count = buttons.Length;
                command.getTab("Cmd-bottom", true);
                tabSelected = true;
                command.Br().addHr(new DsnDataHr().H(8f));
                command.Br();
                var reroll = command.addButtonT<aBtnNel>(new DsnDataButton
                {
                    name = ButtonName,
                    title = ButtonName,
                    skin_title = TranslatorResource.CookingReroll,
                    skin = "row_center",
                    w = Math.Min(220f, bottom.use_w),
                    h = 32f,
                    fnClick = button => Reroll(__instance, button)
                });
                session.RerollButton = reroll;
                for (int i = 0; i < count; i++)
                {
                    var nativeButton = buttons.Get(i);
                    // 数量控件用上下键修改数字；保留它的原生导航，经左右键到制作按钮后再向下重随机。
                    if (!(nativeButton is aBtnNumCounter))
                        nativeButton.setNaviB(reroll, false, true);
                }
                reroll.setNaviT(submit, false, true);
                reroll.setNaviB(submit, false, true);
                // 已存在的 Tab 只重新计算高度，不能再次 assignTab，否则会重复登记布局块。
                bottom.cropBounds(-1f, bottom.get_sheight_px());
                command.endTab(false, false);
                tabSelected = false;
                command.rowRemakeCheck(true);
                if (selected != null)
                    selected.Select();
            }
            catch (Exception ex)
            {
                Disable(__instance, ex, refresh: true);
            }
            finally
            {
                if (tabSelected)
                    command.endTab(false, false);
            }
        }

        private static bool Reroll(UiCraftBase ui, aBtn button)
        {
            try
            {
                if (!ui.handle || button.isLocked() || Convert.ToInt32(StateField.GetValue(ui)) != RecipeConfirm
                    || !TryGetPreview(ui, out var session, out var dish))
                    return false;
                session.Preview.Update(dish, GetIngredients(ui), true);
                Refresh(ui);
                button.Select();
                IN.clearPushDown();
                return true;
            }
            catch (Exception ex)
            {
                Disable(ui, ex, refresh: true);
                return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UiCraftBase), "fnConfirmRecipeCreate")]
        private static bool ConfirmPrefix(UiCraftBase __instance, ref bool __result, out CookingEffectCommit __state)
        {
            __state = null;
            try
            {
                if (!TryGetPreview(__instance, out var session, out var dish))
                    return true;
                var ingredients = GetIngredients(__instance);
                __state = session.Preview.BeginCommit(dish, ingredients);
                if (__state == null)
                {
                    session.Preview.Update(dish, ingredients);
                    Refresh(__instance);
                    __result = false;
                    return false;
                }
                session.Commit = __state;
                return true;
            }
            catch (Exception ex)
            {
                Disable(__instance, ex, refresh: true);
                __result = false;
                return false;
            }
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UiCraftBase), "addCompletionToStorage")]
        private static void StoragePrefix(UiCraftBase __instance)
        {
            if (Sessions.TryGetValue(__instance, out var session) && session.Commit != null)
                session.Commit.StorageStarted = true;
        }

        [HarmonyFinalizer]
        [HarmonyPatch(typeof(UiCraftBase), "fnConfirmRecipeCreate")]
        private static Exception ConfirmFinalizer(UiCraftBase __instance, bool __result, Exception __exception, CookingEffectCommit __state)
        {
            __state?.Finish(__exception == null && __result);
            if (Sessions.TryGetValue(__instance, out var session))
            {
                session.Commit = null;
                if (__result || __state?.StorageStarted == true)
                    session.Preview.Clear();
            }
            return __exception;
        }
    }
}
