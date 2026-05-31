using BetterExperience.HConfigGUI.Bindings;
using BetterExperience.HLogSpace;
using BetterExperience.HotkeyManager;
using System;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.HConfigGUI.Editor
{
    public class HotkeyEditSession
    {
        public IEntryBinding Entry { get; set; }
        public HotkeyEditState State { get; set; } = HotkeyEditState.Idle;
        public Hotkey OriginalValue { get; set; }
        public Hotkey WorkingValue { get; set; }
        public HotkeyChord WorkingChord { get; set; }
        public GamepadChord PreviewGamepadChord { get; set; }
        public KeyboardChord PreviewKeyboardChord { get; set; }
        public bool IsEditing => Entry != null && State != HotkeyEditState.Idle;
        public bool IsRecording => State != HotkeyEditState.Idle && State != HotkeyEditState.Expanded;

        public HotkeyEditSession()
        {
            GuiPipe.OnEntryValueReset += e => CancelEdit();
        }

        public void Update()
        {
            switch (State)
            {
                case HotkeyEditState.Idle:
                case HotkeyEditState.Expanded:
                    return;
                case HotkeyEditState.WaitingPress:
                    DealWaitingPress();
                    break;
                case HotkeyEditState.Recording:
                    DealRecording();
                    break;
                case HotkeyEditState.WaitingConfirm:
                    DealWaitingConfirm();
                    break;
                default:
                    break;
            }
        }

        public void Clear()
        {
            if (Entry?.Value is Hotkey hotkey)
                hotkey.Valid = true;
            Hotkey.GlobalValid = true;

            Entry = null;
            State = HotkeyEditState.Idle;
            OriginalValue = null;
            WorkingValue = null;
            WorkingChord = null;
            PreviewGamepadChord = null;
            PreviewKeyboardChord = null;
        }

        public void BeginEdit(IEntryBinding entry)
        {
            if (entry == null || State != HotkeyEditState.Idle)
                return;

            var current = entry.Value as Hotkey;
            if (current == null)
                return;

            Entry = entry;
            State = HotkeyEditState.Expanded;
            OriginalValue = current;
            WorkingValue = OriginalValue.Clone();
            WorkingValue.Valid = false;
            WorkingChord = null;
            PreviewGamepadChord = null;
            PreviewKeyboardChord = null;
        }

        public void BeginRecord(HotkeyChord chord)
        {
            if (!IsEditing || State != HotkeyEditState.Expanded)
                return;

            Hotkey.GlobalValid = false;
            OriginalValue.Valid = false;
            WorkingChord = chord;
            State = HotkeyEditState.WaitingPress;
        }

        public void CancelRecord()
        {
            if (!IsEditing || State == HotkeyEditState.Expanded)
                return;

            Hotkey.GlobalValid = true;
            OriginalValue.Valid = true;
            WorkingValue = OriginalValue.Clone();
            WorkingValue.Valid = false;
            WorkingChord = null;
            State = HotkeyEditState.Expanded;
        }

        public void CancelEdit()
        {
            Clear();
        }

        public void ConfirmEdit(EntryChangeSink changeSink)
        {
            if (!IsEditing || State != HotkeyEditState.WaitingConfirm)
            {
                Clear();
                return;
            }

            ConfirmRecord(changeSink);
            Clear();
        }

        public void ConfirmRecord(EntryChangeSink changeSink)
        {
            if (!IsEditing || State != HotkeyEditState.WaitingConfirm)
            {
                CancelRecord();
                return;
            }

            if (PreviewGamepadChord != null && PreviewKeyboardChord != null)
            {
                if (PreviewGamepadChord.IsValid && PreviewKeyboardChord.IsValid)
                {
                    HLog.Warn($"Both gamepad and keyboard inputs are detected, which is ambiguous");
                }
                else if (PreviewGamepadChord.IsValid)
                {
                    WorkingChord.Chord = PreviewGamepadChord;
                }
                else if (PreviewKeyboardChord.IsValid)
                {
                    WorkingChord.Chord = PreviewKeyboardChord;
                }
            }

            if (WorkingChord != null)
                WorkingValue.Add(WorkingChord);

            WorkingValue.RemoveInvalidHotkey();
            if (!WorkingValue.HasSameHotkey(OriginalValue) && WorkingValue.Count > 0)
            {
                WorkingValue.Valid = true;
                changeSink.SetValue(Entry, WorkingValue);
            }

            Hotkey.GlobalValid = true;
            OriginalValue.Valid = true;

            WorkingChord = null;
            PreviewGamepadChord = null;
            PreviewKeyboardChord = null;
            OriginalValue = Entry.Value as Hotkey;
            WorkingValue = OriginalValue.Clone();
            WorkingValue.Valid = false;
            State = HotkeyEditState.Expanded;
        }

        public void SetWorkingChord(HotkeyChord chord, EntryChangeSink changeSink)
        {
            if (!IsEditing)
                return;

            ConfirmRecord(changeSink);
            BeginRecord(chord);
        }

        public void AddChord(HotkeyChord chord, EntryChangeSink changeSink)
        {
            if (!IsEditing)
                return;

            ConfirmRecord(changeSink);
            WorkingValue.Add(chord);
            BeginRecord(chord);
        }

        public void RemoveChord(HotkeyChord chord, EntryChangeSink changeSink)
        {
            if (!IsEditing)
                return;

            if (WorkingChord == chord)
                CancelRecord();

            WorkingValue.Remove(chord);
            State = HotkeyEditState.WaitingConfirm;
            ConfirmRecord(changeSink);
        }

        public void DealWaitingPress()
        {
            if (!IsEditing || State != HotkeyEditState.WaitingPress)
                return;

            var snapshot = CaptureHotkeyInput();
            if (snapshot == null || !snapshot.HasAnyPressed)
                return;

            PreviewGamepadChord = snapshot.GamepadChord;
            PreviewKeyboardChord = snapshot.KeyboardChord;
            State = HotkeyEditState.Recording;
        }

        public void DealRecording()
        {
            if (!IsEditing || State != HotkeyEditState.Recording)
                return;

            var snapshot = CaptureHotkeyInput();
            if (snapshot == null)
                return;

            if (!snapshot.HasAnyPressed)
            {
                State = HotkeyEditState.WaitingConfirm;
                return;
            }

            if (snapshot.GamepadChord.IsValid && snapshot.GamepadChord.Count >= PreviewGamepadChord.Count)
            {
                PreviewGamepadChord = snapshot.GamepadChord;
                PreviewKeyboardChord.Clear();
                WorkingChord.Chord = PreviewGamepadChord;
                return;
            }

            if (!PreviewKeyboardChord.IsValid || snapshot.KeyboardChord.IsValid)
            {
                PreviewKeyboardChord = snapshot.KeyboardChord;
                PreviewGamepadChord.Clear();
                WorkingChord.Chord = PreviewKeyboardChord;
                return;
            }
        }

        public void DealWaitingConfirm()
        {
            if (!IsEditing || State != HotkeyEditState.WaitingConfirm)
                return;

            var snapshot = CaptureHotkeyInput();
            if (snapshot?.HasAnyPressed == true)
            {
                PreviewGamepadChord = snapshot.GamepadChord;
                PreviewKeyboardChord = snapshot.KeyboardChord;
                if (PreviewGamepadChord.IsValid)
                    WorkingChord.Chord = PreviewGamepadChord;
                else
                    WorkingChord.Chord = PreviewKeyboardChord;

                State = HotkeyEditState.Recording;
                return;
            }

            if (!WorkingChord.IsValid)
            {
                PreviewGamepadChord.Clear();
                PreviewKeyboardChord.Clear();
                State = HotkeyEditState.Recording;
                return;
            }
        }

        public HotkeyInputSnapshot CaptureHotkeyInput()
        {
            if (!IsEditing)
                return null;

            var snapshot = new HotkeyInputSnapshot
            {
                HasAnyPressed = false,
                GamepadChord = new GamepadChord(WorkingValue.UnityService),
                KeyboardChord = new KeyboardChord(WorkingValue.UnityService)
            };

            var gamepad = WorkingValue.UnityService.GamepadCurrent;
            if (gamepad != null)
            {
                foreach (GamepadButton button in Enum.GetValues(typeof(GamepadButton)))
                {
                    var controller = gamepad[button];
                    if (controller == null)
                        continue;

                    if (controller.wasPressedThisFrame || controller.isPressed)
                    {
                        snapshot.GamepadChord.AddButton(button);
                        snapshot.HasAnyPressed = true;
                    }
                }
            }

            var keyboard = WorkingValue.UnityService.KeyboardCurrent;
            if (keyboard == null)
                return snapshot;

            foreach (var key in keyboard.allKeys)
            {
                if (key == null)
                    continue;

                if (key.isPressed)
                {
                    snapshot.HasAnyPressed = true;
                    if (KeyboardModifierTrigger.IsModifierKey(key.keyCode))
                    {
                        snapshot.KeyboardChord.AddModifier(key.keyCode);
                        continue;
                    }
                }

                if (key.wasPressedThisFrame)
                {
                    snapshot.KeyboardChord.SetMainKey(key.keyCode);
                    snapshot.HasAnyPressed = true;
                }
            }

            return snapshot;
        }
    }
}
