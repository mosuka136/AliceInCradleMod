using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HProvider;

namespace BetterExperience.HConfigGUI.Editor
{
    public class EntryEditor
    {
        public ValueEditorRegistry Registry { get; }
        public GuiStateStore State { get; }
        public EntryChangeSink ChangeSink { get; }

        public float RearBlankWidth => UnityGui.ButtonStyle.CalcSize(UnityGui.GetContent(TranslatorResource.Reset)).x;
        public UnityGuiProvider UnityGui { get; }

        public EntryEditor(ValueEditorRegistry registry, GuiStateStore state, EntryChangeSink changeSink, UnityGuiProvider unity)
        {
            Registry = registry;
            State = state;
            ChangeSink = changeSink;
            UnityGui = unity;

            GuiPipe.OnEntryValueChanged += e => { State.DeleteText(e); State.DeleteBool(e); };
            GuiPipe.OnEntryValueReset += e => { State.DeleteText(e); State.DeleteBool(e); };
        }

        public void Render(IEntryBinding entry)
        {
            var editor = Registry.GetEditor(entry);

            if (UnityGui == null || entry == null)
                return;

            State.SetFloat(GuiStateStore.RearBlankWidthKey, RearBlankWidth);

            UnityGui.BeginHorizontal();
            UnityGui.Label(UnityGui.GetContent(entry.Name, entry.Description), UnityGui.Width(State.GetFloat(GuiStateStore.LeadingBlankWidthKey)));
            editor.DrawValue(entry, State, ChangeSink);
            if (UnityGui.Button(TranslatorResource.Reset, UnityGui.ExpandWidth(false)))
                ChangeSink.ResetValue(entry);
            UnityGui.EndHorizontal();

            editor.DrawExtra(entry, State, ChangeSink);
        }
    }
}
