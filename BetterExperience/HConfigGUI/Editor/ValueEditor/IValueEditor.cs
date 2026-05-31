using BetterExperience.HConfigGUI.Bindings;

namespace BetterExperience.HConfigGUI.Editor.ValueEditor
{
    public interface IValueEditor
    {
        bool CanEdit(IEntryBinding entry);
        void DrawValue(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink);
        void DrawExtra(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink);
    }
}
