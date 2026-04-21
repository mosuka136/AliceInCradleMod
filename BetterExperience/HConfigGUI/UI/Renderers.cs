using BetterExperience.HAdapter;
using BetterExperience.HEnumHelper;
using BetterExperience.HotkeyManager;
using System;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI.UI
{
    public interface IEntryRenderer
    {
        ViewModel Context { get; }
        void Render(UiEntryModel entry);
    }

    public abstract class BaseEntryRenderer : IEntryRenderer
    {
        protected IGuiLayout Layout { get; }

        public ViewModel Context { get; }

        protected BaseEntryRenderer(ViewModel context, IGuiLayout layout)
        {
            Context = context;
            Layout = layout;
        }

        public void Render(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            Layout.BeginHorizontal();
            Layout.Label(new GuiContentAdapter(entry.Name, entry.Description), Layout.Width(LayoutResource.GetLabelWidth(Context.Sheet)));
            RenderEntry(entry);
            ResetButtonRenderer.Render(Context, Layout, entry);
            Layout.EndHorizontal();

            RenderAfterRow(entry);
        }

        public virtual void RenderAfterRow(UiEntryModel entry) { }

        public abstract void RenderEntry(UiEntryModel entry);
    }

    public class BoolRenderer : BaseEntryRenderer
    {
        public BoolRenderer(ViewModel context, IGuiLayout layout) : base(context, layout) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (entry.ValueType != typeof(bool))
                return;

            bool value = (entry.CacheValue ?? entry.Value) as bool? ?? false;
            var newValue = Layout.Toggle(value, value ? TranslatorResource.On : TranslatorResource.Off, Layout.ExpandWidth(true));
            if (newValue != value)
            {
                Context.SetValue(entry, newValue);
            }
        }
    }

    public class StringRenderer : BaseEntryRenderer
    {
        public StringRenderer(ViewModel context, IGuiLayout layout) : base(context, layout) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (entry.ValueType != typeof(string))
                return;

            var value = (entry.CacheValue ?? entry.Value) as string ?? string.Empty;
            var newValue = Layout.TextField(value, Layout.ExpandWidth(true));
            if (newValue != value)
            {
                Context.SetValue(entry, newValue, Context.StringDuration);
            }
        }
    }

    public class NumberRenderer : BaseEntryRenderer
    {
        public NumberRenderer(ViewModel context, IGuiLayout layout) : base(context, layout) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!entry.ValueType.IsPrimitive || entry.ValueType == typeof(bool) || entry.ValueType == typeof(char))
                return;

            string valueStr;
            if (!string.IsNullOrEmpty(entry.CacheValueString))
            {
                valueStr = entry.CacheValueString;
            }
            else
            {
                var stringParseResult = Parser.Parse<string>(entry.CacheValue ?? entry.Value);
                valueStr = stringParseResult.Success ? stringParseResult.Value : string.Empty;
                entry.CacheValueString = valueStr;
            }

            var newValueStr = Layout.TextField(valueStr, Layout.ExpandWidth(true));
            if (newValueStr != valueStr)
            {
                entry.CacheValueString = newValueStr;

                var newValueParseResult = Parser.Parse(entry.ValueType, newValueStr);
                if (newValueParseResult.Success)
                {
                    Context.SetValue(entry, newValueParseResult.Value, Context.NumberDuration);
                }
            }
        }
    }

    public class EnumRenderer : BaseEntryRenderer
    {
        private readonly Dictionary<UiEntryModel, (Array values, List<int> mapIndex, string[] names)> _cacheEnumInfo = new Dictionary<UiEntryModel, (Array values, List<int> mapIndex, string[] names)>();

        public EnumRenderer(ViewModel context, IGuiLayout layout) : base(context, layout) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!entry.ValueType.IsEnum)
                return;

            var value = (entry.CacheValue ?? entry.Value) as Enum;
            var clicked = Layout.Button(EnumHelper.GetDescription(entry.ValueType, value), Layout.ExpandWidth(true));
            if (clicked)
                Context.OpenedEnumEntry = Context.OpenedEnumEntry == entry ? null : entry;
        }

        public override void RenderAfterRow(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (Context.OpenedEnumEntry != entry)
                return;

            if (!entry.ValueType.IsEnum)
                return;

            var value = (entry.CacheValue ?? entry.Value) as Enum;
            var type = entry.ValueType;

            Array values;
            List<int> mapIndexList;
            string[] names;
            if (!_cacheEnumInfo.ContainsKey(entry))
            {
                values = Enum.GetValues(type);

                mapIndexList = new List<int>();
                var namesList = new List<string>();
                for (int i = 0; i < values.Length; i++)
                {
                    var enumValue = (Enum)values.GetValue(i);
                    if (EnumHelper.IsDisplay(type, enumValue))
                    {
                        string description = EnumHelper.GetDescription(type, enumValue);
                        namesList.Add(description);

                        mapIndexList.Add(i);
                    }
                }

                names = namesList.ToArray();

                _cacheEnumInfo[entry] = (values, mapIndexList, names);
            }
            else
            {
                var cache = _cacheEnumInfo[entry];
                values = cache.values;
                mapIndexList = cache.mapIndex;
                names = cache.names;
            }

            int currentIndex = Array.IndexOf(values, value);
            currentIndex = mapIndexList.IndexOf(currentIndex);
            currentIndex = currentIndex >= 0 ? currentIndex : 0;

            Layout.BeginHorizontal();
            Layout.Space(LayoutResource.GetLabelWidth(Context.Sheet));

            Layout.BeginVertical(GuiStyleAdapter.BoxStyle);
            int newIndex = Layout.SelectionGrid(currentIndex, names, 1, Layout.ExpandWidth(true));
            Layout.EndVertical();

            Layout.Space(ResetButtonRenderer.GetWidth());
            Layout.EndHorizontal();

            if (currentIndex != newIndex)
            {
                Context.SetValue(entry, values.GetValue(mapIndexList[newIndex]));

                Context.OpenedEnumEntry = null;
            }
        }
    }

    public class SliderRenderer : BaseEntryRenderer
    {
        public SliderRenderer(ViewModel context, IGuiLayout layout) : base(context, layout) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!entry.ValueType.IsPrimitive || entry.ValueType == typeof(bool) || entry.ValueType == typeof(char))
                return;

            var metadata = entry.Metadata as UiSliderMetadata;
            if (metadata == null)
            {
                Layout.Label(new GuiContentAdapter(TranslatorResource.InvalidSliderMetadata), Layout.ExpandWidth(true));
                return;
            }

            var parseResult = Parser.Parse<float>(entry.CacheValue ?? entry.Value);
            var value = parseResult.Success ? parseResult.Value : metadata.Min;
            var displayValue = MathHelper.Clamp(value, metadata.Min, metadata.Max);
            var newSliderValue = Layout.HorizontalSlider(
                displayValue, 
                metadata.Min,
                metadata.Max,
                StyleResource.Instance.SliderStyle,
                StyleResource.Instance.SliderThumbStyle,
                Layout.ExpandWidth(true));

            if (MathHelper.Approximately(displayValue, newSliderValue))
                newSliderValue = value;

            if (!MathHelper.Approximately(value, newSliderValue))
            {
                if (metadata.Step > 0f)
                    newSliderValue = MathHelper.Round(newSliderValue / metadata.Step) * metadata.Step;

                entry.CacheValueString = Parser.Parse<string>(newSliderValue).Value;

                Context.SetValue(entry, Parser.Parse(entry.ValueType, newSliderValue).Value, Context.SliderDuration);
            }

            string textValueStr;
            if (!string.IsNullOrEmpty(entry.CacheValueString))
            {
                textValueStr = entry.CacheValueString;
            }
            else
            {
                var textValueStrParseResult = Parser.Parse<string>(entry.CacheValue ?? entry.Value);
                textValueStr = textValueStrParseResult.Success ? textValueStrParseResult.Value : string.Empty;
                entry.CacheValueString = textValueStr;
            }

            var newTextValueStr = Layout.TextField(textValueStr, Layout.MinWidth(50f), Layout.ExpandWidth(false));

            if (newTextValueStr != textValueStr)
            {
                entry.CacheValueString = newTextValueStr;

                var newTextValueParseResult = Parser.Parse(entry.ValueType, newTextValueStr);
                if (newTextValueParseResult.Success)
                {
                    Context.SetValue(entry, newTextValueParseResult.Value, Context.SliderDuration);
                }
            }
        }
    }

    public class HotkeyRenderer : BaseEntryRenderer
    {
        public HotkeyRenderer(ViewModel context, IGuiLayout layout) : base(context, layout) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!typeof(Hotkey).IsAssignableFrom(entry.ValueType))
                return;

            var value = (entry.CacheValue ?? entry.Value) as Hotkey;
            string displayText = value != null ? value.ToString() : string.Empty;
            var clicked = Layout.Button(displayText, Layout.ExpandWidth(true));
            if (clicked)
            {
                ApplyHotkey();

                if (Context.OpenedHotkeyEntry == entry)
                {
                    Context.OpenedHotkeyEntry = null;
                    Context.RecordingHotkey = null;
                }
                else
                    Context.OpenedHotkeyEntry = entry;
            }
        }

        public override void RenderAfterRow(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!typeof(Hotkey).IsAssignableFrom(entry.ValueType))
                return;

            if (Context.OpenedHotkeyEntry != entry)
                return;

            Hotkey value;
            if (entry.CacheValue == null)
            {
                value = new Hotkey(entry.Value as Hotkey) { Valid = false };
                entry.CacheValue = value;
            }
            else
                value = entry.CacheValue as Hotkey;

            HotkeyChord needRemoveHotkey = null;
            Layout.BeginHorizontal();
            Layout.Space(LayoutResource.GetLabelWidth(Context.Sheet));
            Layout.BeginVertical(GuiStyleAdapter.BoxStyle);
            foreach (var hotkey in value.Hotkeys)
            {
                Layout.BeginHorizontal();

                Layout.Button(hotkey.ToString(), Layout.ExpandWidth(true));
                if (Layout.Button(Context.RecordingHotkey == hotkey ? TranslatorResource.Apply : TranslatorResource.Record, Layout.ExpandWidth(false)))
                {
                    if (Context.RecordingHotkey == hotkey)
                    {
                        ApplyHotkey();
                        Context.RecordingHotkey = null;
                    }
                    else
                    {
                        (entry.Value as Hotkey).Valid = false;
                        hotkey.Clear();
                        Context.RecordingHotkey = hotkey;
                    }
                }
                if (value.Count > 1)
                {
                    if (Layout.Button(TranslatorResource.Remove, Layout.ExpandWidth(false)))
                    {
                        if (Context.RecordingHotkey == hotkey)
                        {
                            Context.RecordingHotkey = null;
                        }
                        needRemoveHotkey = hotkey;
                    }
                }

                Layout.EndHorizontal();
            }
            value.Remove(needRemoveHotkey);

            Layout.BeginHorizontal();

            if (Layout.Button(TranslatorResource.Add, Layout.ExpandWidth(true)))
            {
                var newHotkeyChord = new HotkeyChord();
                value.Add(newHotkeyChord);
                Context.RecordingHotkey = newHotkeyChord;
            }

            Layout.EndHorizontal();

            Layout.EndVertical();
            Layout.EndHorizontal();
        }

        public void ApplyHotkey()
        {
            var entry = Context.OpenedHotkeyEntry;
            if (entry == null)
                return;

            var newValue = entry.CacheValue as Hotkey;
            if (!(entry.Value as Hotkey).HasSameHotkey(newValue))
            {
                newValue.RemoveInvalidHotkey();
                newValue.Valid = true;
                Context.SetValue(entry, entry.CacheValue);
            }
            entry.CacheValue = null;
            (entry.Value as Hotkey).Valid = true;
        }
    }

    public static class ResetButtonRenderer
    {
        public static void Render(ViewModel context, IGuiLayout layout, UiEntryModel entry)
        {
            if (context == null || entry == null)
                return;

            var clicked = layout.Button(TranslatorResource.Reset, layout.ExpandWidth(false));
            if (clicked)
                context.ResetValue(entry);
        }

        public static float GetWidth()
        {
            return GuiStyleAdapter.ButtonStyle.CalcSize(new GuiContentAdapter(TranslatorResource.Reset)).x;
        }
    }

    public class TableRenderer
    {

        public ViewModel Context { get; }
        private IGuiLayout Layout { get; }

        public BoolRenderer BoolEntryRenderer { get; }
        public StringRenderer StringEntryRenderer { get; }
        public NumberRenderer NumberEntryRenderer { get; }
        public EnumRenderer EnumEntryRenderer { get; }
        public SliderRenderer SliderEntryRenderer { get; }
        public HotkeyRenderer HotkeyRenderer { get; }

        public TableRenderer(ViewModel context, IGuiLayout layout)
        {
            Context = context;
            Layout = layout;

            BoolEntryRenderer = new BoolRenderer(context, layout);
            StringEntryRenderer = new StringRenderer(context, layout);
            NumberEntryRenderer = new NumberRenderer(context, layout);
            EnumEntryRenderer = new EnumRenderer(context, layout);
            SliderEntryRenderer = new SliderRenderer(context, layout);
            HotkeyRenderer = new HotkeyRenderer(context, layout);
        }

        public void Render(UiTableModel table)
        {
            if (Context == null || table == null)
                return;

            Layout.BeginVertical(GuiStyleAdapter.BoxStyle);
            Layout.BeginHorizontal();
            Layout.Label(new GuiContentAdapter(table.Name, table.Description), StyleResource.Instance.TableTitleStyle, Layout.ExpandWidth(true));
            Layout.EndHorizontal();

            foreach (var entry in table)
            {
                var type = entry.ValueType;
                if (type == typeof(bool))
                {
                    BoolEntryRenderer.Render(entry);
                }
                else if (type == typeof(string))
                {
                    StringEntryRenderer.Render(entry);
                }
                else if (type.IsPrimitive)
                {
                    var metadata = entry.Metadata as UiSliderMetadata;
                    if (metadata != null)
                    {
                        SliderEntryRenderer.Render(entry);
                    }
                    else
                    {
                        NumberEntryRenderer.Render(entry);
                    }
                }
                else if (type.IsEnum)
                {
                    EnumEntryRenderer.Render(entry);
                }
                else if (typeof(Hotkey).IsAssignableFrom(type))
                {
                    HotkeyRenderer.Render(entry);
                }
            }

            Layout.Space(10f);
            Layout.EndVertical();
        }
    }

    public class SheetRenderer
    {
        public ViewModel Context { get; }
        private IGuiLayout Layout { get; }

        public TableRenderer TableRenderer { get; }

        public SheetRenderer(ViewModel context, IGuiLayout layout)
        {
            Context = context;
            Layout = layout;
            TableRenderer = new TableRenderer(context, layout);
        }

        public void Render(UiSheetModel sheet)
        {
            if (Context == null || sheet == null)
                return;

            foreach (var table in sheet)
            {
                TableRenderer.Render(table);
                Layout.Space(10f);
            }
        }
    }

    public class ToastRenderer
    {
        public ViewModel Context { get; }

        public ToastRenderer(ViewModel context)
        {
            Context = context;
        }

        public void Render()
        {
            if (Context == null)
                return;

            if (string.IsNullOrEmpty(Context.ToastMessage))
                return;

            float remaining = Context.ToastEndTime - UnityTimeAdapter.RealtimeSinceStartup;
            if (remaining <= 0f)
            {
                Context.ToastMessage = null;
                return;
            }

            float alpha = remaining < Context.ToastFadeDuration ? remaining / Context.ToastFadeDuration : 1f;
            var previousColor = GuiAdapter.Color;
            GuiAdapter.Color = new Color(1f, 1f, 1f, alpha);

            var content = new GuiContentAdapter(Context.ToastMessage);
            var size = StyleResource.Instance.ToastStyle.CalcSize(content);
            float toastWidth = MathHelper.Min(size.x + 20f, Context.WindowRect.width - 20f);
            float toastHeight = size.y + 4f;
            float x = (Context.WindowRect.width - toastWidth) / 2f;
            float y = MathHelper.Max(0f, Context.WindowRect.height - toastHeight - 30f);

            GuiAdapter.Label(new Rect(x, y, toastWidth, toastHeight), content, StyleResource.Instance.ToastStyle.Style);
            GuiAdapter.Color = previousColor;
        }
    }

    public class TooltipRenderer
    {
        public ViewModel Context { get; }

        public TooltipRenderer(ViewModel context)
        {
            Context = context;
        }

        public void Render()
        {
            if (Context == null)
                return;

            if (string.IsNullOrEmpty(GuiAdapter.Tooltip))
                return;

            var tooltipContent = new GuiContentAdapter(GuiAdapter.Tooltip);
            float maxTooltipWidth = Context.WindowRect.width * 0.6f;
            float tooltipHeight = StyleResource.Instance.TooltipStyle.CalcHeight(tooltipContent, maxTooltipWidth);

            var mousePosition = UnityEventAdapter.Current.MousePosition;
            float x = MathHelper.Clamp(mousePosition.x + 15f, 0f, Context.WindowRect.width - maxTooltipWidth);
            float y = MathHelper.Clamp(mousePosition.y + 15f, 0f, Context.WindowRect.height - tooltipHeight);

            GuiAdapter.Label(new Rect(x, y, maxTooltipWidth, tooltipHeight), tooltipContent, StyleResource.Instance.TooltipStyle.Style);
        }
    }
}
