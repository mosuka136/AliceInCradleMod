using BetterExperience.HConfigGUI.Bindings;
using System.Linq;
using BetterExperience.HTranslatorSpace;
using System;
using BetterExperience.HLogSpace;

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
            foreach (var handler in (OnEntryValueChanged?.GetInvocationList() ?? Array.Empty<Delegate>()).Cast<Action<IEntryBinding>>())
            {
                try
                {
                    handler?.Invoke(entry);
                }
                catch (Exception ex)
                {
                    HLog.Error($"Error invoking OnEntryValueChanged handler: {handler?.Method.Name}", ex);
                }
            }
        }

        public static void InvokeOnEntryValueReset(IEntryBinding entry)
        {
            foreach (var handler in (OnEntryValueReset?.GetInvocationList() ?? Array.Empty<Delegate>()).Cast<Action<IEntryBinding>>())
            {
                try
                {
                    handler?.Invoke(entry);
                }
                catch (Exception ex)
                {
                    HLog.Error($"Error invoking OnEntryValueReset handler: {handler?.Method.Name}", ex);
                }
            }
        }
    }
}
