using BetterExperience.BConfigManager;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HTranslatorSpace;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace BetterExperience.HConfigGUI
{
    public class ViewModel
    {
        private readonly Dictionary<UiEntryModel, float> _entryDelayApplyTime = new Dictionary<UiEntryModel, float>();

        public Rect WindowRect { get; set; }

        public UiSheetModel Sheet { get; }
        public UiEntryModel OpenedEnumEntry { get; set; }
        public HotkeyInputSystem ConfigUIHotkey { get; set; }
        public string ToastMessage { get; set; }
        public float ToastEndTime { get; set; }
        public float ToastDuration { get; set; } = 2f;
        public float ToastFadeDuration { get; set; } = 0.5f;
        public float StringDuration { get; set; } = 0.5f;
        public float NumberDuration { get; set; } = 0.5f;
        public float SliderDuration { get; set; } = 0.5f;

        public ViewModel()
        {
            Sheet = new UiSheetModel();

            Translator.DefaultLanguage = ConfigManager.SetLanguage.Value;
            SetUiHotkey(ConfigManager.ConfigUIHotkey.Value);

            ConfigManager.ConfigUIHotkey.OnValueChanged += (s, e) => SetUiHotkey(e);
            ConfigManager.SetLanguage.OnValueChanged += (s, e) =>
            {
                Translator.DefaultLanguage = e;
                LayoutResource.InvalidateLayout();
            };
        }

        public void Update(float deltaTime)
        {
            UpdateValueTime(deltaTime);
        }

        public void UpdateValueTime(float deltaTime)
        {
            var keys = _entryDelayApplyTime.Keys.ToList();
            foreach (var key in keys)
            {
                _entryDelayApplyTime[key] -= deltaTime;
            }

            var entriesToUpdate = _entryDelayApplyTime.Where(kv => kv.Value <= 0f).Select(kv => kv.Key).ToList();
            foreach (var entry in entriesToUpdate)
            {
                if (entry.CacheValue == null)
                    continue;

                entry.Value = entry.CacheValue;
                _entryDelayApplyTime.Remove(entry);
                ShowToast(TranslatorResource.Changed + entry.Name, ToastDuration);
            }
        }

        public void SetValue(UiEntryModel entry, object value, float delaySec = 0f)
        {
            if (entry == null)
                return;

            if (delaySec > 0f)
            {
                _entryDelayApplyTime[entry] = delaySec;
                entry.CacheValue = value;
                return;
            }
            entry.Value = value;
            ShowToast(TranslatorResource.Changed + entry.Name, ToastDuration);
        }

        public void ResetValue(UiEntryModel entry)
        {
            if (entry == null)
                return;

            _entryDelayApplyTime.Remove(entry);
            entry.Value = entry.DefaultValue;
            ShowToast(TranslatorResource.ResetDone + entry.Name, ToastDuration);
        }

        public void SetUiHotkey(string hotkeyText)
        {
            var configuredHotkey = hotkeyText;
            if (!HotkeyInputSystem.TryParse(configuredHotkey, out var hotkey))
            {
                HLog.Warn("Invalid Hotkey: " + configuredHotkey);
                configuredHotkey = "F1";
                HotkeyInputSystem.TryParse(configuredHotkey, out hotkey);
            }
            ConfigUIHotkey = hotkey;

            HLog.Info("Config UI hotkey set: " + configuredHotkey);
        }

        public void ShowToast(string message, float duration)
        {
            ToastMessage = message;
            ToastEndTime = Time.realtimeSinceStartup + duration;
        }
    }
}
