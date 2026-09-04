using BetterExperience.BConfigManager;
using BetterExperience.BLogSpace;
using BetterExperience.BPatchGUI;
using evt;
using HarmonyLib;
using m2d;
using nel;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using UnityEngine;
using XX;

namespace BetterExperience.Patches
{
    [HarmonyPatch]
    internal static class BattleEnemyPreviewPatch
    {
        private const BindingFlags Members = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
        private static readonly ConditionalWeakTable<UILpSummon, Session> Sessions = new ConditionalWeakTable<UILpSummon, Session>();
        private static readonly ConditionalWeakTable<DesignerRowMem, Session> ScrollRows = new ConditionalWeakTable<DesignerRowMem, Session>();
        private static readonly FieldInfo LeftField = typeof(UILpSummon).GetField("BxDL", Members);
        private static readonly FieldInfo TitleField = typeof(UILpSummon).GetField("BxT", Members);
        private static readonly FieldInfo BottomField = typeof(UILpSummon).GetField("BxDB", Members);
        private static readonly FieldInfo RightField = typeof(UILpSummon).GetField("BxDR", Members);
        private static readonly FieldInfo DescriptionField = typeof(UILpSummon).GetField("StbDesc", Members);
        private static readonly FieldInfo ScrollMaterialField = typeof(ScrollBox).GetField("MtrMask", Members);
        private static readonly FieldInfo ScrollMeshField = typeof(ScrollBox).GetField("MdBackground", Members);
        private static bool _layoutAvailable;

        private sealed class Session
        {
            internal UiBoxDesigner Left;
            internal UiBoxDesigner Bottom;
            internal IDesignerPosSetableBlock Title;
            internal IDesignerPosSetableBlock Right;
            internal string Signature;
            internal bool Applied;
            internal bool Failed;
            internal float NextCheck;
            internal float Height = 120f;
            internal int OriginalStencil;
            internal bool OriginalNoWriteMask;
            internal float OriginalHorizontalMargin;
            internal float OriginalVerticalMargin;
            internal ValotileRenderer ScrollMaskRenderer;
            internal MeshRenderer ScrollMeshRenderer;
            internal aBtnMeter[] ScrollBars;
            internal float ScrollBarAlpha = float.NaN;
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UILpSummon), nameof(UILpSummon.activate))]
        private static void Activate(UILpSummon __instance)
        {
            Refresh(__instance, true);
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UILpSummon), nameof(UILpSummon.run))]
        private static void Run(UILpSummon __instance)
        {
            Refresh(__instance, false);
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(DesignerRowMem), nameof(DesignerRowMem.setAlpha))]
        private static void SyncRowAlpha(DesignerRowMem __instance, float value)
        {
            if (!ScrollRows.TryGetValue(__instance, out var session) || !session.Applied || session.Failed) return;
            try { SetScrollBarsAlpha(session, value); }
            catch (Exception exception)
            {
                session.Failed = true;
                BLog.Error("Unable to synchronize battle preview scrollbar opacity.", exception);
            }
        }

        [HarmonyPostfix]
        [HarmonyPatch(typeof(UILpSummon), nameof(UILpSummon.releaseTextCache))]
        private static void Invalidate(UILpSummon __instance)
        {
            if (Sessions.TryGetValue(__instance, out var session)) session.Signature = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UILpSummon), nameof(UILpSummon.deactivate))]
        private static void Deactivate(UILpSummon __instance)
        {
            if (!Sessions.TryGetValue(__instance, out var session)) return;
            try { Restore(__instance, session); }
            catch (Exception exception) { BLog.Error("Unable to restore battle preview layout.", exception); }
            session.Signature = null;
        }

        [HarmonyPrefix]
        [HarmonyPatch(typeof(UILpSummon), nameof(UILpSummon.destruct))]
        private static void Destroy(UILpSummon __instance)
        {
            if (Sessions.TryGetValue(__instance, out var session)) ScrollRows.Remove(session.Left.getRowManager());
            Sessions.Remove(__instance);
        }

        [HarmonyCleanup]
        private static void Cleanup(Exception __exception)
        {
            if (__exception == null) return;
            _layoutAvailable = false;
            BLog.Error("Battle enemy preview registration failed.", __exception);
        }

        private static void Refresh(UILpSummon ui, bool force)
        {
            Session session = null;
            try
            {
                bool enabled = _layoutAvailable && ConfigManager.EnableBattleEnemyPreview?.Value == true;
                if (!Sessions.TryGetValue(ui, out session))
                {
                    if (!enabled) return;
                    var left = (UiBoxDesigner)LeftField.GetValue(ui);
                    session = new Session
                    {
                        Left = left,
                        Bottom = (UiBoxDesigner)BottomField.GetValue(ui),
                        Title = (IDesignerPosSetableBlock)TitleField.GetValue(ui),
                        Right = (IDesignerPosSetableBlock)RightField.GetValue(ui),
                        OriginalStencil = left.stencil_ref,
                        OriginalNoWriteMask = left.no_write_mask,
                        OriginalHorizontalMargin = left.margin_in_lr,
                        OriginalVerticalMargin = left.margin_in_tb
                    };
                    Sessions.Add(ui, session);
                }
                if (!enabled || session.Failed)
                {
                    Restore(ui, session);
                    session.Signature = null;
                    return;
                }
                SyncScrollRendering(session);
                float now = Time.realtimeSinceStartup;
                if (!force && now < session.NextCheck) return;
                session.NextCheck = now + 0.25f;
                string script = ui.Reader.GetManager()?.getSummonerScript(ui.Reader.key, out _);
                var context = Capture(ui, script ?? "", out var signature);
                if (!force && signature == session.Signature) return;
                var preview = BattleEnemyPreview.Build(script, context);
                string text = Describe(ui, preview);
                Apply(session, text);
                session.Signature = signature;
            }
            catch (Exception exception)
            {
                BLog.Error("Battle enemy preview disabled for this panel.", exception);
                if (session == null) return;
                session.Failed = true;
                try { Restore(ui, session); }
                catch (Exception restoreException) { BLog.Error("Unable to restore native battle description.", restoreException); }
            }
        }

        internal static BattleEnemyPreviewContext Capture(UILpSummon ui, string script, out string signature)
        {
            var night = ui.M2D.NightCon;
            var quest = ui.Reader.QEntry;
            int danger = night.cur_dlevel(ui.Reader);
            bool isNight = night.isNight();
            int divisor = ui.Lp.skill_difficulty_restrict == 0 ? 2 : 1;
            int cycle = Math.Min(5, danger / 16);
            int remainder = danger % 16;
            int attributeDanger = (int)(night.cur_dlevel(ui.Reader, !quest.valid || quest.nattr == ENATTR.NORMAL) * (quest.valid && quest.is_rescue ? 1.8f : 1f));
            float budget = (attributeDanger - 16) * 1.2f / 16f + 1f;
            if (isNight) budget = (budget - 3f) * 0.7f;
            if (attributeDanger >= 80) budget += isNight ? 1 : 3;
            int weather = night.current_weather_bit;
            if (quest.valid && quest.weather > WeatherItem.WEATHER.NORMAL) weather |= 1 << ((int)quest.weather & 31);
            float level = night.getDangerLevel();
            var context = new BattleEnemyPreviewContext
            {
                DangerLevel = level,
                Night = isNight,
                AdditionalCount = (isNight ? 2 + X.IntC(cycle * 2.5f) : (cycle != 0 ? X.IntC(cycle * 1.77f) + (remainder >= 4 ? 1 : 0) : (remainder + 1) / 3)) / divisor,
                CountCapAddition = !isNight ? cycle * 2 / divisor + (remainder > 3 ? 1 : 0) : (int)(cycle * 2.5 + 2) / divisor,
                ThunderCapacity = Math.Max(0, ui.Reader.get_thunder_odable_def() + ((weather & (1 << (int)WeatherItem.WEATHER.THUNDER)) != 0 ? Math.Max((int)(level + 0.4f), 1) : 0)),
                AttributeDanger = attributeDanger,
                AttributeBudget = attributeDanger < 16 ? 0 : (int)Math.Max(0, budget),
                AttributeKindMaximum = attributeDanger >= 96 ? 3 : attributeDanger >= 56 ? 2 : 1,
                QuestEnemy = quest.valid && quest.fix_enemykind >= 0 ? (ENEMYID?)quest.fix_enemykind : null,
                QuestAttributes = quest.valid ? quest.nattr : ENATTR.NORMAL,
                QuestAttributeBudget = quest.valid && quest.nattr != ENATTR.NORMAL && quest.nattr_addable_max > 0,
                SpecialBattle = quest.valid && quest.is_rescue || ui.Lp.is_sudden_puppetrevenge
            };
            if (context.QuestEnemy.HasValue)
            {
                float factor = (danger + 7) / 16f;
                context.QuestMinimum = isNight ? Math.Min(2 + (int)(factor * 1.1200000047683716), 7) : Math.Min(2 + (int)(factor * 0.6600000262260437), 5);
            }
            var variables = new CsvVariableContainer(EV.getVariableContainer());
            if (!EV.isActive()) variables.removeTemp();
            variables.define("_here", ui.Reader.key, false);
            variables.define("_map", ui.Lp.Mp.key, false);
            var names = BattleEnemyPreviewScript.ValueToken.Matches(script).Cast<System.Text.RegularExpressions.Match>()
                .Select(match => match.Value).Distinct().OrderBy(value => value, StringComparer.Ordinal).ToList();
            // 变量值可能继续引用其他变量或只读表达式，需要一并复制。
            for (int i = 0; i < names.Count && i < 4096; i++)
            {
                string name = names[i];
                string value = variables.Get(name);
                if (value != null)
                {
                    context.Variables[name] = value;
                    foreach (System.Text.RegularExpressions.Match match in BattleEnemyPreviewScript.ValueToken.Matches(value))
                        if (!names.Contains(match.Value)) names.Add(match.Value);
                }
                double? number = CaptureValue(name, night);
                if (number.HasValue) context.Values[name] = number.Value;
            }
            var key = new StringBuilder(script);
            key.Append('|').Append(danger).Append('|').Append(level.ToString("R", CultureInfo.InvariantCulture)).Append('|').Append(attributeDanger)
                .Append('|').Append(weather).Append('|').Append(divisor).Append('|').Append(isNight).Append('|').Append(context.ThunderCapacity)
                .Append('|').Append(quest.fix_enemykind).Append('|').Append((uint)quest.nattr).Append('|').Append(quest.nattr_addable_max)
                .Append('|').Append(context.SpecialBattle).Append('|').Append(ConfigManager.SetLanguage?.Value).Append('|').Append(TX.getCurrentFamilyName());
            foreach (var value in context.Variables.OrderBy(value => value.Key, StringComparer.Ordinal)) key.Append('|').Append(value.Key).Append('=').Append(value.Value);
            foreach (var value in context.Values.OrderBy(value => value.Key, StringComparer.Ordinal)) key.Append('|').Append(value.Key).Append('=').Append(value.Value.ToString("R", CultureInfo.InvariantCulture));
            signature = key.ToString();
            return context;
        }

        private static double? CaptureValue(string token, NightController night)
        {
            switch (token)
            {
                case "difficulty": return DIFF.I;
                case "PVV": return GF.getC("PHASE") * 100U + GF.getC("PHASEV");
                case "is_night": return night.isNight() ? 1 : 0;
                case "danger_level": return night.getDangerMeterVal(true);
            }
            if (token.StartsWith("GFC[", StringComparison.Ordinal) && token.EndsWith("]", StringComparison.Ordinal)) return GF.getC(token.Substring(4, token.Length - 5));
            if (token.StartsWith("SF[", StringComparison.Ordinal) && token.EndsWith("]", StringComparison.Ordinal)) return COOK.getSF(token.Substring(3, token.Length - 4));
            return null;
        }

        private static string Describe(UILpSummon ui, BattleEnemyPreviewSnapshot snapshot)
        {
            using (var text = TX.PopBld())
            using (var grade = TX.PopBld())
            {
                grade.Add(ui.Reader.grade);
                int obtainable = ui.Reader.getObtainableGrade(ui.Lp.Mp);
                if (obtainable < ui.Reader.grade) grade.AddTxA("Summoner__obtainable_dangerousness").TxRpl(obtainable);
                text.AddTxA("Summoner__descryption").TxRpl(grade).TxRpl(TranslatorResource.BattleEnemyPreviewTitle.ToString())
                    .TxRpl("\n" + Format(snapshot)).TxRpl(ui.Reader.getOverdriveCapacityStr());
                if (ui.Reader.only_night) text.AppendTxA("Summoner_can_battle_in_night");
                return text.ToString();
            }
        }

        internal static string Format(BattleEnemyPreviewSnapshot snapshot)
        {
            var text = new StringBuilder();
            foreach (var entry in snapshot.Entries)
            {
                string name = entry.EnemyId.HasValue
                    ? TranslatorResource.GetEnemyDisplayName(entry.EnemyId.Value | (entry.Overdrive ? ENEMYID._OVERDRIVE_FLAG : 0), entry.Attributes)
                    : entry.EnemyKey;
                text.Append(Escape(name)).Append(" × ").Append(Count(entry.Minimum, entry.Maximum));
                if (entry.Source == BattleEnemySource.Follower) text.Append(TranslatorResource.BattleEnemyPreviewFollower);
                text.Append('\n');
                if (entry.PossibleAttributes != ENATTR.NORMAL)
                    text.Append("  ").Append(TranslatorResource.BattleEnemyPreviewAttributes).Append(Escape(TranslatorResource.GetEnemyAttributeName(entry.PossibleAttributes))).Append('\n');
            }
            text.Append(TranslatorResource.BattleEnemyPreviewTotal).Append(Count(snapshot.Minimum, snapshot.Maximum)).Append('\n');
            if (snapshot.RandomAddition > 0) text.Append(TranslatorResource.BattleEnemyPreviewAdditional).Append(snapshot.RandomAddition).Append('\n');
            if (snapshot.ThunderCapacity > 0) text.Append(TranslatorResource.BattleEnemyPreviewThunder).Append(snapshot.ThunderCapacity).Append('\n');
            if (snapshot.RandomAttributes > 0) text.Append(TranslatorResource.BattleEnemyPreviewAttributeSlots).Append(snapshot.RandomAttributes).Append('\n');
            if (snapshot.Entries.Any(entry => entry.Maximum.HasValue && entry.Minimum != entry.Maximum)) text.Append(TranslatorResource.BattleEnemyPreviewShared).Append('\n');
            if (snapshot.Incomplete) text.Append(TranslatorResource.BattleEnemyPreviewIncomplete).Append('\n');
            if (snapshot.DynamicReinforcements) text.Append(TranslatorResource.BattleEnemyPreviewDynamic).Append('\n');
            return text.ToString();
        }

        private static string Count(int minimum, int? maximum) => !maximum.HasValue ? TranslatorResource.BattleEnemyPreviewUnknown.ToString()
            : minimum == maximum.Value ? minimum.ToString(CultureInfo.InvariantCulture) : minimum + "–" + maximum.Value;
        private static string Escape(string value) => value.Replace("&", "&amp;").Replace("<", "&lt;").Replace(">", "&gt;");

        private static void Apply(Session session, string text)
        {
            // 修改原生面板前先标记，确保部分操作失败时也能恢复。
            session.Applied = true;
            var left = session.Left;
            left.Clear();
            left.use_scroll = true;
            left.stencil_ref = 249;
            // 原生 UiBox 已有背景，Designer 的透明背景不应再次使用写入颜色的遮罩材质。
            left.no_write_mask = true;
            left.margin_in_lr = 18f;
            left.margin_in_tb = 12f;
            left.WH(320f, 280f);
            left.init();
            ConfigureScrollRendering(session);
            var paragraph = left.addP(new DsnDataP
            {
                name = "descl", text = text, size = 13f, html = true,
                swidth = left.use_w - 16f, sheight = 0f, alignx = ALIGN.LEFT, aligny = ALIGNY.TOP,
                text_auto_wrap = true, text_auto_condense = false, TxCol = C32.d2c(uint.MaxValue)
            }, true);
            session.Height = Math.Max(120f, Math.Min(280f, paragraph.get_sheight_px() + 24f));
            left.WH(320f, session.Height);
            left.fine_scroll_inner |= 1;
            // 使用原生自动滚动，让长名单无需鼠标聚焦或额外快捷键也能完整阅读。
            left.getScrollBox()?.startAutoScroll(180);
        }

        private static void ConfigureScrollRendering(Session session)
        {
            var scroll = session.Left.getScrollBox();
            var mesh = (MeshDrawer)ScrollMeshField.GetValue(scroll);
            var renderer = scroll.GetComponent<MeshRenderer>();
            if (mesh == null || renderer == null) throw new InvalidOperationException("Battle preview scroll mask is unavailable.");

            // 只写入裁剪信息，避免透明滚动区域被绘制成黑色矩形。
            // 同时更新 ScrollBox 的材质缓存，防止后续滚动重绘恢复原来的材质。
            var material = MTRX.getMtr(Shd: MTRX.ShaderMeshMaskTransparent, stencil_ref: scroll.stencil_ref);
            ScrollMaterialField.SetValue(scroll, material);
            mesh.setMaterial(material);
            renderer.sharedMaterial = material;

            // 现场文字通过 Valotile 在相机后处理阶段绘制，遮罩必须使用同一流程。
            // ScrollBox 原本只使用 MeshRenderer，其遮罩会被稍后绘制的 UiBox 背景覆盖。
            session.ScrollMeshRenderer = renderer;
            session.ScrollMaskRenderer = ValotileRenderer.Create(mesh, renderer, session.Left.use_valotile);
            session.ScrollBars = scroll.GetComponentsInChildren<aBtnMeter>(true);
            var row = session.Left.getRowManager();
            ScrollRows.Remove(row);
            ScrollRows.Add(row, session);
            // 新建滚动条默认完全不透明，首帧就应采用原生文字的当前透明度。
            var box = session.Left.getBox();
            session.ScrollBarAlpha = float.NaN;
            SetScrollBarsAlpha(session, box.show_delaying ? 0f : session.Left.alpha * box.alpha * box.alpha);
            SyncScrollRendering(session);
        }

        private static void SetScrollBarsAlpha(Session session, float value)
        {
            if (session.ScrollBarAlpha == value) return;
            // 原生内容区每次淡入、淡出时同步更新，不受预览数据的刷新间隔影响。
            foreach (var bar in session.ScrollBars)
                SetScrollBarAlpha(bar.get_Skin(), value, session.Left.scroll_normal_color, session.Left.scroll_push_color);
            session.ScrollBarAlpha = value;
        }

        internal static void SetScrollBarAlpha(ButtonSkin skin, float value, uint normalColor, uint pushedColor)
        {
            if (skin == null) return;
            // 原生滚动条的主体直接使用这两种颜色，单改 skin.alpha 只会影响部分渐变。
            if (skin is ButtonSkinMeterScroll scrollSkin)
                scrollSkin.setColor(C32.MulA(normalColor, value), C32.MulA(pushedColor, value));
            skin.alpha = value;
            // 按钮更新可能早于面板动画，立即重绘可避免旧透明度残留一帧。
            skin.Fine();
        }

        private static void SyncScrollRendering(Session session)
        {
            if (!session.Applied || session.ScrollMaskRenderer == null) return;
            bool useValotile = session.Left.use_valotile;
            session.ScrollMaskRenderer.enabled = useValotile;
            // ScrollBox 再次启用时会自行打开 MeshRenderer，显式关闭它以避免重复绘制。
            session.ScrollMeshRenderer.enabled = !useValotile;
            // 滚动条也必须跟随相同的绘制模式，避免被原生背景遮住。
            foreach (var bar in session.ScrollBars) bar.use_valotile = useValotile;
        }

        private static void Restore(UILpSummon ui, Session session)
        {
            if (!session.Applied) return;
            var left = session.Left;
            ScrollRows.Remove(left.getRowManager());
            left.Clear();
            left.use_scroll = false;
            left.stencil_ref = session.OriginalStencil;
            left.no_write_mask = session.OriginalNoWriteMask;
            left.margin_in_lr = session.OriginalHorizontalMargin;
            left.margin_in_tb = session.OriginalVerticalMargin;
            left.WH(320f, 120f);
            left.init();
            left.addP(new DsnDataP
            {
                name = "descl", text = ((STB)DescriptionField.GetValue(ui)).ToString(), size = 13f, html = true,
                swidth = left.use_w, sheight = left.use_h, alignx = ALIGN.LEFT, aligny = ALIGNY.MIDDLE, TxCol = C32.d2c(uint.MaxValue)
            });
            session.Height = 120f;
            session.Applied = false;
        }

        [HarmonyTranspiler]
        [HarmonyPatch(typeof(UILpSummon), nameof(UILpSummon.run))]
        private static IEnumerable<CodeInstruction> Layout(IEnumerable<CodeInstruction> instructions)
        {
            var original = instructions.ToList();
            _layoutAvailable = TryRewriteLayout(original, out var rewritten);
            if (!_layoutAvailable) BLog.Warn("Battle enemy preview disabled: native layout calls did not match ver030d.");
            return rewritten;
        }

        internal static bool TryRewriteLayout(IList<CodeInstruction> original, out List<CodeInstruction> rewritten)
        {
            var target = typeof(M2BoxOneLine).GetMethod(nameof(M2BoxOneLine.fineBoxPosOnMapWH));
            var replacement = typeof(BattleEnemyPreviewPatch).GetMethod(nameof(Position), BindingFlags.Static | BindingFlags.NonPublic);
            rewritten = original.ToList();
            if (original.Count(code => code.Calls(target)) != 4 || LeftField == null || TitleField == null || BottomField == null || RightField == null || DescriptionField == null
                || ScrollMaterialField == null || ScrollMeshField == null) return false;
            rewritten = new List<CodeInstruction>();
            foreach (var code in original)
            {
                if (!code.Calls(target)) { rewritten.Add(code); continue; }
                var receiver = new CodeInstruction(OpCodes.Ldarg_0);
                receiver.labels.AddRange(code.labels);
                receiver.blocks.AddRange(code.blocks);
                rewritten.Add(receiver);
                rewritten.Add(new CodeInstruction(OpCodes.Call, replacement));
            }
            return true;
        }

        private static void Position(IDesignerPosSetableBlock box, M2DBase m2d, Vector4 position, float width, float height, bool first, float x, float y, UILpSummon ui)
        {
            if (Sessions.TryGetValue(ui, out var session) && session.Applied)
            {
                float halfExtra = (session.Height - 120f) * 0.5f;
                if (ReferenceEquals(box, session.Title) || ReferenceEquals(box, session.Right)) y += halfExtra;
                if (ReferenceEquals(box, session.Bottom)) y -= halfExtra;
                height = Math.Max(200f + halfExtra * 2f, 2f * (82f + halfExtra + session.Bottom.get_sheight_px()));
            }
            M2BoxOneLine.fineBoxPosOnMapWH(box, m2d, position, width, height, first, x, y);
        }
    }
}
