using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HProvider;

namespace BetterExperience.HConfigGUI.Editor.ValueEditor
{
    public class StringEditor : IValueEditor
    {
        public float DelayApplyDuration { get; set; } = 0.5f;

        public UnityGuiProvider UnityGui { get; }

        public StringEditor(UnityGuiProvider unityGui)
        {
            UnityGui = unityGui;
        }

        public bool CanEdit(IEntryBinding entry)
        {
            return entry.ValueType == typeof(string);
        }

        public void DrawValue(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
            var key = state.GetKey(entry, "_string");
            var value = state.GetText(key, entry.Value as string);
            if (value == null)
                return;

            string newValue = UnityGui.TextField(value, UnityGui.ExpandWidth(true));
            if (newValue != value)
            {
                state.SetText(key, newValue);
                changeSink.SetValue(entry, newValue, DelayApplyDuration);
            }
        }

        public void DrawExtra(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
        }
    }
}
