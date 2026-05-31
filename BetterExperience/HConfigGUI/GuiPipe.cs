using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HTranslatorSpace;
using System;

namespace BetterExperience.HConfigGUI
{
    public static class GuiPipe
    {
        public static Translator PopupTitle { get; set; }
        public static Action PopupWindowAction { get; set; }
        public static Action ClosePopupWindowAction { get; set; }

        public static event Action<IEntryBinding> OnEntryValueChanged;
        public static event Action<IEntryBinding> OnEntryValueReset;

        public static void InvokeOnEntryValueChanged(IEntryBinding entry)
        {
            OnEntryValueChanged?.Invoke(entry);
        }

        public static void InvokeOnEntryValueReset(IEntryBinding entry)
        {
            OnEntryValueReset?.Invoke(entry);
        }
    }
}
