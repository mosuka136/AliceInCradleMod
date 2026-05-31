using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HProvider;

namespace BetterExperience.HConfigGUI.Editor.ValueEditor
{
    public class UnsupportedEditor : IValueEditor
    {
        public UnityGuiProvider UnityGui { get; }

        public static UnsupportedEditor Instance = new UnsupportedEditor();

        public UnsupportedEditor()
        {
            UnityGui = new UnityGuiProvider();
        }

        public bool CanEdit(IEntryBinding entry)
        {
            return false;
        }

        public void DrawValue(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
            UnityGui.Label("Unsupported type: " + entry.ValueType.FullName);
        }

        public void DrawExtra(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
        }
    }
}
