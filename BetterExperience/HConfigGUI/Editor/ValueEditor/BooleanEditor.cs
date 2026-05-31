using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;

namespace BetterExperience.HConfigGUI.Editor.ValueEditor
{
    public class BooleanEditor : IValueEditor
    {
        public UnityGuiProvider UnityGui { get; }

        public BooleanEditor(UnityGuiProvider unityGui)
        {
            UnityGui = unityGui;
        }

        public bool CanEdit(IEntryBinding entry)
        {
            return entry.ValueType == typeof(bool);
        }

        public void DrawValue(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
            if (entry.Value is bool value)
            {
                bool newValue = UnityGui.Toggle(value, value ? TranslatorResource.On : TranslatorResource.Off, UnityGui.ExpandWidth(true));
                if (newValue != value)
                    changeSink.SetValue(entry, newValue);
            }
        }

        public void DrawExtra(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
        }
    }
}
