using BetterExperience.BConfigManager;
using BetterExperience.HProvider;
using BetterExperience.HConfigGUI.UI;
using BetterExperience.HotkeyManager;
using BetterExperience.HTranslatorSpace;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.HConfigGUI
{
    public class ViewModel
    {
        private readonly Dictionary<UiEntryModel, float> _entryDelayApplyTime = new Dictionary<UiEntryModel, float>();

        public UnityProvider UnityService { get; private set; }
        public UnityGuiProvider UnityGuiService { get; private set; }

        public StyleResource StyleResourceInstance { get; private set; }
        public LayoutResource LayoutResourceInstance { get; private set; }

        public Rect WindowRect { get; set; }
        public float LabelWidth { get; set; } = -1f;

        public UiSheetModel Sheet { get; }
        public UiEntryModel OpenedEnumEntry { get; set; }
        public UiEntryModel OpenedHotkeyEntry { get; set; }
        public HotkeyChord RecordingHotkey { get; set; }
        public Hotkey ConfigUIHotkey { get; set; }
        public string ToastMessage { get; set; }
        public float ToastEndTime { get; set; }
        public float ToastDuration { get; set; } = 2f;
        public float ToastFadeDuration { get; set; } = 0.5f;
        public float StringDuration { get; set; } = 0.5f;
        public float NumberDuration { get; set; } = 0.5f;
        public float SliderDuration { get; set; } = 0.5f;

        public ViewModel()
        {
            UnityService = new UnityProvider();
            UnityGuiService = new UnityGuiProvider();

            StyleResourceInstance = new StyleResource(this);
            LayoutResourceInstance = new LayoutResource(this);

            Sheet = new UiSheetModel();

            ConfigUIHotkey = ConfigManager.ConfigUIHotkey.Value;
            Translator.DefaultLanguage = ConfigManager.SetLanguage.Value;

            ConfigManager.ConfigUIHotkey.OnValueChanged += (s, e) => ConfigUIHotkey = e;
            ConfigManager.SetLanguage.OnValueChanged += (s, e) =>
            {
                Translator.DefaultLanguage = e;
                LabelWidth = -1f;
            };
        }

        public void Update(float deltaTime)
        {
            UpdateValueTime(deltaTime);
            RecordHotkey();
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

        public void RecordHotkey()
        {
            var recordingHotkey = RecordingHotkey;
            if (recordingHotkey == null)
                return;

            if (recordingHotkey.Modifiers == null)
                recordingHotkey.Modifiers = new List<IHotkeyTrigger>();

            var gamepad = UnityService.GamepadCurrent;
            if (gamepad != null)
            {
                foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
                {
                    var gamepadButtonControl = gamepad[button];
                    if (gamepadButtonControl != null && gamepadButtonControl.wasPressedThisFrame)
                    {
                        recordingHotkey.MainKey = new GamepadTrigger(button, UnityService);
                        return;
                    }
                }
            }

            var keyboard = UnityService.KeyboardCurrent;
            if (keyboard == null)
                return;

            if (recordingHotkey.MainKey == null)
                recordingHotkey.ClearModifiers();

            foreach (var key in keyboard.allKeys)
            {
                if (key == null)
                    continue;

                if (key.isPressed && KeyboardModifierTrigger.IsModifierKey(key.keyCode))
                {
                    recordingHotkey.AddModifier(key.keyCode);
                    continue;
                }

                if (key.wasPressedThisFrame)
                {
                    recordingHotkey.MainKey = new KeyboardTrigger(key.keyCode, UnityService);
                    return;
                }
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

        public void ShowToast(string message, float duration)
        {
            ToastMessage = message;
            ToastEndTime = UnityService.RealtimeSinceStartup + duration;
        }
    }
}
