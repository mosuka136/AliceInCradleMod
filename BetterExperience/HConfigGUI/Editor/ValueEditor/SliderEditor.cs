using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;

namespace BetterExperience.HConfigGUI.Editor.ValueEditor
{
    public class SliderEditor : NumberEditor, IValueEditor
    {
        public UnityProvider UnityService { get; }
        public StyleResource StyleProvider { get; }

        public SliderEditor(UnityGuiProvider unityGui, UnityProvider unityService, StyleResource styleProvider)
            : base(unityGui)
        {
            UnityService = unityService;
            StyleProvider = styleProvider;
        }

        public override bool CanEdit(IEntryBinding entry)
        {
            return base.CanEdit(entry) && entry.Metadata is UiSliderMetadata;
        }

        public override void DrawValue(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
            if (!entry.ValueType.IsPrimitive || entry.ValueType == typeof(bool) || entry.ValueType == typeof(char))
                return;

            var metadata = entry.Metadata as UiSliderMetadata;
            if (metadata == null)
            {
                UnityGui.Label(UnityGui.GetContent(TranslatorResource.InvalidSliderMetadata), UnityGui.ExpandWidth(true));
                return;
            }

            var key = state.GetKey(entry, "_slider");
            var valueString = state.GetText(key, entry.Value.ToString());
            var value = float.TryParse(valueString, out var result) ? result : metadata.Min;
            var displayValue = UnityService.Clamp(value, metadata.Min, metadata.Max);
            var newSliderValue = UnityGui.HorizontalSlider(
                displayValue,
                metadata.Min,
                metadata.Max,
                StyleProvider.SliderStyle,
                StyleProvider.SliderThumbStyle,
                UnityGui.ExpandWidth(true));

            // 显示值会被夹在滑条范围内；如果用户未拖动滑条，应保留原始越界值，避免单纯绘制就改写配置。
            if (UnityService.Approximately(displayValue, newSliderValue))
                newSliderValue = value;

            if (!UnityService.Approximately(value, newSliderValue) && metadata.Step > 0f)
                newSliderValue = UnityService.Round(newSliderValue / metadata.Step) * metadata.Step;

            var sliderString = newSliderValue.ToString();
            var newValueString = UnityGui.TextField(sliderString, UnityGui.MinWidth(50f), UnityGui.ExpandWidth(false));

            if (valueString != newValueString)
            {
                state.SetText(key, newValueString);
                SetValue(entry, newValueString, changeSink);
            }
        }
    }
}
