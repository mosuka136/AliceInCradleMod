using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HConfigGUI.Resource;
using BetterExperience.HotkeyManager;
using BetterExperience.HProvider;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI.Editor.ValueEditor
{
    public class HotkeyEditor : IValueEditor
    {
        public HotkeyEditSession Session { get; }
        public IUnityGuiProvider UnityGui { get; }
        public StyleResource StyleProvider { get; }

        public HotkeyEditor(IUnityGuiProvider unityGui, StyleResource styleProvider)
        {
            Session = new HotkeyEditSession();
            UnityGui = unityGui;
            StyleProvider = styleProvider;
        }

        public bool CanEdit(IEntryBinding entry)
        {
            return typeof(Hotkey).IsAssignableFrom(entry.ValueType);
        }

        public void DrawValue(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
            if (!typeof(Hotkey).IsAssignableFrom(entry.ValueType))
                return;

            var valueString = GetHotkeyDisplayString(entry, state);
            if (valueString == null)
                return;

            if (UnityGui.Button(valueString, UnityGui.ExpandWidth(true)))
            {
                if (Session.Entry == entry)
                    Session.ConfirmEdit(changeSink);
                else
                {
                    Session.ConfirmEdit(changeSink);
                    Session.BeginEdit(entry);
                }
            }
        }

        public void DrawExtra(IEntryBinding entry, GuiStateStore state, EntryChangeSink changeSink)
        {
            if (Session.Entry != entry)
                return;

            Session.Update();

            var value = Session.WorkingValue;
            if (value == null)
                return;

            UnityGui.BeginHorizontal();
            UnityGui.Space(state.GetFloat(GuiStateStore.LeadingBlankWidthKey, 0f));
            UnityGui.BeginVertical(UnityGui.BoxStyle);

            var chords = new List<HotkeyChord>(value.Hotkeys);
            foreach (var chord in chords)
            {
                UnityGui.BeginHorizontal();

                UnityGui.Button(chord.ToString(), UnityGui.ExpandWidth(true));

                if (UnityGui.Button(TranslatorResource.Record, UnityGui.ExpandWidth(false)))
                {
                    chord.Clear();
                    Session.SetWorkingChord(chord, changeSink);
                    SetPopupWindow(state, changeSink);
                }

                if (value.Count > 1 && UnityGui.Button(TranslatorResource.Remove, UnityGui.ExpandWidth(false)))
                    Session.RemoveChord(chord, changeSink);

                UnityGui.EndHorizontal();
            }

            UnityGui.BeginHorizontal();
            if (UnityGui.Button(TranslatorResource.Add, UnityGui.ExpandWidth(true)))
            {
                Session.AddChord(new HotkeyChord(value.UnityService), changeSink);
                SetPopupWindow(state, changeSink);
            }
            UnityGui.EndHorizontal();

            UnityGui.EndVertical();
            UnityGui.EndHorizontal();
        }

        public void SetPopupWindow(GuiStateStore state, EntryChangeSink changeSink)
        {
            state.SetBool(GuiStateStore.IsPopupOpenKey, true);
            GuiPipe.PopupTitle = TranslatorResource.RecordHotkeyPopupTitle;
            GuiPipe.PopupWindowAction = () => RecordHotkey(state, changeSink);
            GuiPipe.ClosePopupWindowAction = Session.CancelRecord;
        }

        public void RecordHotkey(GuiStateStore state, EntryChangeSink changeSink)
        {
            if (!Session.IsRecording)
                return;

            Session.Update();

            UnityGui.FlexibleSpace();
            UnityGui.Label(Session.WorkingChord.ToString(), StyleProvider.RecordingHotkeyLabelStyle, UnityGui.ExpandWidth(true));

            UnityGui.FlexibleSpace();
            if (UnityGui.Button(TranslatorResource.Apply, UnityGui.ExpandWidth(true)))
            {
                Session.ConfirmRecord(changeSink);
                state.SetBool(GuiStateStore.IsPopupOpenKey, false);
            }
        }

        public string GetHotkeyDisplayString(IEntryBinding entry, GuiStateStore state)
        {
            if (Session.Entry == entry)
                return Session.WorkingValue?.ToString() ?? string.Empty;

            var key = state.GetKey(entry, "_hotkey");
            return state.GetText(key, entry.Value.ToString());
        }
    }
}
