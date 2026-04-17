using BetterExperience.HEnumHelper;
using System;
using System.Collections.Generic;
using UnityEngine;

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

            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(entry.Name, entry.Description), GUILayout.Width(LayoutResource.GetLabelWidth(Context.Sheet)));
            RenderEntry(entry);
            ResetButtonRenderer.Render(Context, entry);
            GUILayout.EndHorizontal();

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
            var newValue = GUILayout.Toggle(value, value ? TranslatorResource.On : TranslatorResource.Off, GUILayout.ExpandWidth(true));
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
            var newValue = GUILayout.TextField(value, GUILayout.ExpandWidth(true));
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

            var newValueStr = GUILayout.TextField(valueStr, GUILayout.ExpandWidth(true));
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
            var clicked = GUILayout.Button(EnumHelper.GetDescription(entry.ValueType, value), GUILayout.ExpandWidth(true));
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

            GUILayout.BeginHorizontal();
            GUILayout.Space(LayoutResource.GetLabelWidth(Context.Sheet));

            GUILayout.BeginVertical("box");
            int newIndex = GUILayout.SelectionGrid(currentIndex, names, 1, GUILayout.ExpandWidth(true));
            GUILayout.EndVertical();

            GUILayout.Space(ResetButtonRenderer.GetWidth());
            GUILayout.EndHorizontal();

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

            var metadata = entry.Metadata as UiSliderMetadata;
            if (metadata == null)
            {
                GUILayout.Label(TranslatorResource.InvalidSliderMetadata, GUILayout.ExpandWidth(true));
                return;
            }

            var parseResult = Parser.Parse<float>(entry.CacheValue ?? entry.Value);
            var value = parseResult.Success ? parseResult.Value : metadata.Min;
            var displayValue = Mathf.Clamp(value, metadata.Min, metadata.Max);
            var newSliderValue = GUILayout.HorizontalSlider(
                displayValue, 
                metadata.Min,
                metadata.Max,
                StyleResource.Instance.SliderStyle,
                StyleResource.Instance.SliderThumbStyle,
                GUILayout.ExpandWidth(true));

            if (Mathf.Approximately(displayValue, newSliderValue))
                newSliderValue = value;

            if (!Mathf.Approximately(value, newSliderValue))
            {
                if (metadata.Step > 0f)
                    newSliderValue = Mathf.Round(newSliderValue / metadata.Step) * metadata.Step;

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

            var newTextValueStr = GUILayout.TextField(textValueStr, GUILayout.MinWidth(50f), GUILayout.ExpandWidth(false));

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

    public static class ResetButtonRenderer
    {
        public static void Render(ViewModel context, UiEntryModel entry)
        {
            if (context == null || entry == null)
                return;

            var clicked = GUILayout.Button(TranslatorResource.Reset, GUILayout.ExpandWidth(false));
            if (clicked)
                context.ResetValue(entry);
        }

        public static float GetWidth()
        {
            return GUI.skin.button.CalcSize(new GUIContent(TranslatorResource.Reset)).x;
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

        public TableRenderer(ViewModel context)
        {
            Context = context;

            BoolEntryRenderer = new BoolRenderer(context);
            StringEntryRenderer = new StringRenderer(context);
            NumberEntryRenderer = new NumberRenderer(context);
            EnumEntryRenderer = new EnumRenderer(context);
            SliderEntryRenderer = new SliderRenderer(context);
        }

        public void Render(UiTableModel table)
        {
            if (Context == null || table == null)
                return;

            GUILayout.BeginVertical("box");
            GUILayout.BeginHorizontal();
            GUILayout.Label(new GUIContent(table.Name, table.Description), StyleResource.Instance.TableTitleStyle, GUILayout.ExpandWidth(true));
            GUILayout.EndHorizontal();

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
            }

            GUILayout.Space(10f);
            GUILayout.EndVertical();
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
                GUILayout.Space(10f);
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

            float remaining = Context.ToastEndTime - Time.realtimeSinceStartup;
            if (remaining <= 0f)
            {
                Context.ToastMessage = null;
                return;
            }

            float alpha = remaining < Context.ToastFadeDuration ? remaining / Context.ToastFadeDuration : 1f;
            var previousColor = GUI.color;
            GUI.color = new Color(1f, 1f, 1f, alpha);

            var content = new GUIContent(Context.ToastMessage);
            var size = StyleResource.Instance.ToastStyle.CalcSize(content);
            float toastWidth = Mathf.Min(size.x + 20f, Context.WindowRect.width - 20f);
            float toastHeight = size.y + 4f;
            float x = (Context.WindowRect.width - toastWidth) / 2f;
            float y = Mathf.Max(0, Context.WindowRect.height - toastHeight - 30f);

            GUI.Label(new Rect(x, y, toastWidth, toastHeight), content, StyleResource.Instance.ToastStyle);
            GUI.color = previousColor;
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

            if (string.IsNullOrEmpty(GUI.tooltip))
                return;

            var tooltipContent = new GUIContent(GUI.tooltip);
            float maxTooltipWidth = Context.WindowRect.width * 0.6f;
            float tooltipHeight = StyleResource.Instance.TooltipStyle.CalcHeight(tooltipContent, maxTooltipWidth);

            var mousePosition = Event.current.mousePosition;
            float x = Mathf.Clamp(mousePosition.x + 15f, 0f, Context.WindowRect.width - maxTooltipWidth);
            float y = Mathf.Clamp(mousePosition.y + 15f, 0f, Context.WindowRect.height - tooltipHeight);

            GUI.Label(new Rect(x, y, maxTooltipWidth, tooltipHeight), tooltipContent, StyleResource.Instance.TooltipStyle);
        }
    }
}
