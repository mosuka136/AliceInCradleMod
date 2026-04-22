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
        public ViewModel Context { get; }

        protected BaseEntryRenderer(ViewModel context)
        {
            Context = context;
        }

        public void Render(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            Context.UnityGuiService.BeginHorizontal();
            Context.UnityGuiService.Label(Context.UnityGuiService.GetContent(entry.Name, entry.Description), Context.UnityGuiService.Width(Context.LabelWidth));
            RenderEntry(entry);
            ResetButtonRenderer.Render(Context, entry);
            Context.UnityGuiService.EndHorizontal();

            RenderAfterRow(entry);
        }

        public virtual void RenderAfterRow(UiEntryModel entry) { }

        public abstract void RenderEntry(UiEntryModel entry);
    }

    public class BoolRenderer : BaseEntryRenderer
    {
        public BoolRenderer(ViewModel context) : base(context) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (entry.ValueType != typeof(bool))
                return;

            bool value = (entry.CacheValue ?? entry.Value) as bool? ?? false;
            var newValue = Context.UnityGuiService.Toggle(value, value ? TranslatorResource.On : TranslatorResource.Off, Context.UnityGuiService.ExpandWidth(true));
            if (newValue != value)
            {
                Context.SetValue(entry, newValue);
            }
        }
    }

    public class StringRenderer : BaseEntryRenderer
    {
        public StringRenderer(ViewModel context) : base(context) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (entry.ValueType != typeof(string))
                return;

            var value = (entry.CacheValue ?? entry.Value) as string ?? string.Empty;
            var newValue = Context.UnityGuiService.TextField(value, Context.UnityGuiService.ExpandWidth(true));
            if (newValue != value)
            {
                Context.SetValue(entry, newValue, Context.StringDuration);
            }
        }
    }

    public class NumberRenderer : BaseEntryRenderer
    {
        public NumberRenderer(ViewModel context) : base(context) { }

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

            var newValueStr = Context.UnityGuiService.TextField(valueStr, Context.UnityGuiService.ExpandWidth(true));
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

        public EnumRenderer(ViewModel context) : base(context) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!entry.ValueType.IsEnum)
                return;

            var value = (entry.CacheValue ?? entry.Value) as Enum;
            var clicked = Context.UnityGuiService.Button(EnumHelper.GetDescription(entry.ValueType, value), Context.UnityGuiService.ExpandWidth(true));
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

            Context.UnityGuiService.BeginHorizontal();
            Context.UnityGuiService.Space(Context.LabelWidth);

            Context.UnityGuiService.BeginVertical(Context.UnityGuiService.BoxStyle);
            int newIndex = Context.UnityGuiService.SelectionGrid(currentIndex, names, 1, Context.UnityGuiService.ExpandWidth(true));
            Context.UnityGuiService.EndVertical();

            Context.UnityGuiService.Space(ResetButtonRenderer.GetWidth(Context));
            Context.UnityGuiService.EndHorizontal();

            if (currentIndex != newIndex)
            {
                Context.SetValue(entry, values.GetValue(mapIndexList[newIndex]));

                Context.OpenedEnumEntry = null;
            }
        }
    }

    public class SliderRenderer : BaseEntryRenderer
    {
        public SliderRenderer(ViewModel context) : base(context) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!entry.ValueType.IsPrimitive || entry.ValueType == typeof(bool) || entry.ValueType == typeof(char))
                return;

            var unityService = Context.UnityService;
            var unityGuiService = Context.UnityGuiService;

            var metadata = entry.Metadata as UiSliderMetadata;
            if (metadata == null)
            {
                unityGuiService.Label(unityGuiService.GetContent(TranslatorResource.InvalidSliderMetadata), unityGuiService.ExpandWidth(true));
                return;
            }

            var parseResult = Parser.Parse<float>(entry.CacheValue ?? entry.Value);
            var value = parseResult.Success ? parseResult.Value : metadata.Min;
            var displayValue = unityService.Clamp(value, metadata.Min, metadata.Max);
            var newSliderValue = unityGuiService.HorizontalSlider(
                displayValue, 
                metadata.Min,
                metadata.Max,
                Context.StyleResourceInstance.SliderStyle,
                Context.StyleResourceInstance.SliderThumbStyle,
                unityGuiService.ExpandWidth(true));

            if (unityService.Approximately(displayValue, newSliderValue))
                newSliderValue = value;

            if (!unityService.Approximately(value, newSliderValue))
            {
                if (metadata.Step > 0f)
                    newSliderValue = unityService.Round(newSliderValue / metadata.Step) * metadata.Step;

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

            var newTextValueStr = unityGuiService.TextField(textValueStr, unityGuiService.MinWidth(50f), unityGuiService.ExpandWidth(false));

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
        public HotkeyRenderer(ViewModel context) : base(context) { }

        public override void RenderEntry(UiEntryModel entry)
        {
            if (Context == null || entry == null)
                return;

            if (!typeof(Hotkey).IsAssignableFrom(entry.ValueType))
                return;

            var value = (entry.CacheValue ?? entry.Value) as Hotkey;
            string displayText = value != null ? value.ToString() : string.Empty;
            var clicked = Context.UnityGuiService.Button(displayText, Context.UnityGuiService.ExpandWidth(true));
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
                value = new Hotkey(entry.Value as Hotkey, Context.UnityService) { Valid = false };
                entry.CacheValue = value;
            }
            else
                value = entry.CacheValue as Hotkey;

            var unityGuiService = Context.UnityGuiService;
            HotkeyChord needRemoveHotkey = null;
            unityGuiService.BeginHorizontal();
            unityGuiService.Space(Context.LabelWidth);
            unityGuiService.BeginVertical(unityGuiService.BoxStyle);
            foreach (var hotkey in value.Hotkeys)
            {
                unityGuiService.BeginHorizontal();

                unityGuiService.Button(hotkey.ToString(), unityGuiService.ExpandWidth(true));
                if (unityGuiService.Button(Context.RecordingHotkey == hotkey ? TranslatorResource.Apply : TranslatorResource.Record, unityGuiService.ExpandWidth(false)))
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
                    if (unityGuiService.Button(TranslatorResource.Remove, unityGuiService.ExpandWidth(false)))
                    {
                        if (Context.RecordingHotkey == hotkey)
                        {
                            Context.RecordingHotkey = null;
                        }
                        needRemoveHotkey = hotkey;
                    }
                }

                unityGuiService.EndHorizontal();
            }
            value.Remove(needRemoveHotkey);

            unityGuiService.BeginHorizontal();

            if (unityGuiService.Button(TranslatorResource.Add, unityGuiService.ExpandWidth(true)))
            {
                var newHotkeyChord = new HotkeyChord(Context.UnityService);
                value.Add(newHotkeyChord);
                Context.RecordingHotkey = newHotkeyChord;
            }

            unityGuiService.EndHorizontal();

            unityGuiService.EndVertical();
            unityGuiService.EndHorizontal();
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
        public static void Render(ViewModel context, UiEntryModel entry)
        {
            if (context == null || entry == null)
                return;

            var clicked = context.UnityGuiService.Button(TranslatorResource.Reset, context.UnityGuiService.ExpandWidth(false));
            if (clicked)
                context.ResetValue(entry);
        }

        public static float GetWidth(ViewModel context)
        {
            return context.UnityGuiService.ButtonStyle.CalcSize(context.UnityGuiService.GetContent(TranslatorResource.Reset)).x;
        }
    }

    public class TableRenderer
    {

        public ViewModel Context { get; }

        public BoolRenderer BoolEntryRenderer { get; }
        public StringRenderer StringEntryRenderer { get; }
        public NumberRenderer NumberEntryRenderer { get; }
        public EnumRenderer EnumEntryRenderer { get; }
        public SliderRenderer SliderEntryRenderer { get; }
        public HotkeyRenderer HotkeyRenderer { get; }

        public TableRenderer(ViewModel context)
        {
            Context = context;

            BoolEntryRenderer = new BoolRenderer(context);
            StringEntryRenderer = new StringRenderer(context);
            NumberEntryRenderer = new NumberRenderer(context);
            EnumEntryRenderer = new EnumRenderer(context);
            SliderEntryRenderer = new SliderRenderer(context);
            HotkeyRenderer = new HotkeyRenderer(context);
        }

        public void Render(UiTableModel table)
        {
            if (Context == null || table == null)
                return;

            var unityGuiService = Context.UnityGuiService;
            unityGuiService.BeginVertical(unityGuiService.BoxStyle);
            unityGuiService.BeginHorizontal();
            unityGuiService.Label(unityGuiService.GetContent(table.Name, table.Description), Context.StyleResourceInstance.TableTitleStyle, unityGuiService.ExpandWidth(true));
            unityGuiService.EndHorizontal();

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

            unityGuiService.Space(10f);
            unityGuiService.EndVertical();
        }
    }

    public class SheetRenderer
    {
        public ViewModel Context { get; }

        public TableRenderer TableRenderer { get; }

        public SheetRenderer(ViewModel context)
        {
            Context = context;
            TableRenderer = new TableRenderer(context);
        }

        public void Render(UiSheetModel sheet)
        {
            if (Context == null || sheet == null)
                return;

            foreach (var table in sheet)
            {
                TableRenderer.Render(table);
                Context.UnityGuiService.Space(10f);
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

            float remaining = Context.ToastEndTime - Context.UnityService.RealtimeSinceStartup;
            if (remaining <= 0f)
            {
                Context.ToastMessage = null;
                return;
            }

            var unityGuiService = Context.UnityGuiService;
            float alpha = remaining < Context.ToastFadeDuration ? remaining / Context.ToastFadeDuration : 1f;
            var previousColor = unityGuiService.Color;
            unityGuiService.Color = unityGuiService.GetColor(1f, 1f, 1f, alpha);

            var content = unityGuiService.GetContent(Context.ToastMessage);
            var size = Context.StyleResourceInstance.ToastStyle.CalcSize(content);
            float toastWidth = Context.UnityService.Min(size.x + 20f, Context.WindowRect.width - 20f);
            float toastHeight = size.y + 4f;
            float x = (Context.WindowRect.width - toastWidth) / 2f;
            float y = Context.UnityService.Max(0f, Context.WindowRect.height - toastHeight - 30f);

            unityGuiService.Label(unityGuiService.GetRect(x, y, toastWidth, toastHeight), content, Context.StyleResourceInstance.ToastStyle);
            unityGuiService.Color = previousColor;
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

            var unityGuiService = Context.UnityGuiService;

            if (string.IsNullOrEmpty(unityGuiService.Tooltip))
                return;

            var tooltipContent = unityGuiService.GetContent(unityGuiService.Tooltip);
            float maxTooltipWidth = Context.WindowRect.width * 0.6f;
            float tooltipHeight = Context.StyleResourceInstance.TooltipStyle.CalcHeight(tooltipContent, maxTooltipWidth);

            var mousePosition = Context.UnityService.CurrentMousePosition;
            float x = Context.UnityService.Clamp(mousePosition.x + 15f, 0f, Context.WindowRect.width - maxTooltipWidth);
            float y = Context.UnityService.Clamp(mousePosition.y + 15f, 0f, Context.WindowRect.height - tooltipHeight);

            unityGuiService.Label(unityGuiService.GetRect(x, y, maxTooltipWidth, tooltipHeight), tooltipContent, Context.StyleResourceInstance.TooltipStyle);
        }
    }
}
