using System;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience
{
    public class HotkeyInputSystem
    {
        private List<Chord> _chords = new List<Chord>();
        public bool IsValid { get { return _chords.Count > 0; } }

        public bool WasPressedThisFrame()
        {
            for (int i = 0; i < _chords.Count; i++)
                if (_chords[i].WasPressedThisFrame())
                    return true;
            return false;
        }

        public static bool TryParse(string text, out HotkeyInputSystem hk)
        {
            hk = new HotkeyInputSystem();
            if (string.IsNullOrWhiteSpace(text)) return false;

            var chordStrings = text.Split(',');
            for (int c = 0; c < chordStrings.Length; c++)
            {
                var chordText = chordStrings[c].Trim();
                if (chordText.Length == 0) continue;

                Chord chord;
                if (Chord.TryParse(chordText, out chord))
                    hk._chords.Add(chord);
            }

            return hk.IsValid;
        }

        private struct Chord
        {
            public Trigger Main;
            public Trigger[] Mods;

            public bool WasPressedThisFrame()
            {
                for (int i = 0; i < Mods.Length; i++)
                    if (!Mods[i].IsPressed()) return false;

                return Main.WasPressedThisFrame();
            }

            public static bool TryParse(string chordText, out Chord chord)
            {
                chord = default;

                var raw = chordText.Split('+');
                var parts = new List<string>(raw.Length);
                for (int i = 0; i < raw.Length; i++)
                {
                    var s = raw[i].Trim();
                    if (s.Length > 0) parts.Add(s);
                }
                if (parts.Count == 0) return false;

                string mainStr = parts[parts.Count - 1];
                Trigger main;
                if (!Trigger.TryParse(mainStr, out main)) return false;

                var mods = new List<Trigger>(parts.Count - 1);
                for (int i = 0; i < parts.Count - 1; i++)
                {
                    Trigger t;
                    if (!Trigger.TryParse(parts[i], out t)) return false;
                    mods.Add(t);
                }

                chord = new Chord { Main = main, Mods = mods.ToArray() };
                return true;
            }
        }

        private struct Trigger
        {
            public TriggerKind Kind;
            public Key Key;
            public Key LeftKey, RightKey;
            public GamepadButton Button;
            public bool IsAnySide;

            public bool IsPressed()
            {
                if (Kind == TriggerKind.Keyboard)
                {
                    var kb = Keyboard.current;
                    if (kb == null) return false;

                    if (IsAnySide)
                        return kb[LeftKey].isPressed || kb[RightKey].isPressed;

                    return kb[Key].isPressed;
                }

                if (Kind == TriggerKind.Gamepad)
                {
                    var gp = Gamepad.current;
                    if (gp == null) return false;
                    return gp[Button].isPressed;
                }

                return false;
            }

            public bool WasPressedThisFrame()
            {
                if (Kind == TriggerKind.Keyboard)
                {
                    var kb = Keyboard.current;
                    if (kb == null) return false;

                    if (IsAnySide)
                        return kb[LeftKey].wasPressedThisFrame || kb[RightKey].wasPressedThisFrame;

                    return kb[Key].wasPressedThisFrame;
                }

                if (Kind == TriggerKind.Gamepad)
                {
                    var gp = Gamepad.current;
                    if (gp == null) return false;
                    return gp[Button].wasPressedThisFrame;
                }

                return false;
            }

            public static bool TryParse(string token, out Trigger t)
            {
                t = default;
                if (string.IsNullOrWhiteSpace(token)) return false;

                GamepadButton btn;
                if (TryParseGamepad(token, out btn))
                {
                    t.Kind = TriggerKind.Gamepad;
                    t.Button = btn;
                    return true;
                }

                if (TryParseModifier(token, out t))
                    return true;

                Key key;
                if (Enum.TryParse(token, true, out key))
                {
                    t.Kind = TriggerKind.Keyboard;
                    t.Key = key;
                    t.IsAnySide = false;
                    return true;
                }

                return false;
            }

            public static bool TryParseModifier(string token, out Trigger t)
            {
                t = default;

                if (EqualsI(token, "ctrl") || EqualsI(token, "control"))
                {
                    t.Kind = TriggerKind.Keyboard;
                    t.IsAnySide = true;
                    t.LeftKey = Key.LeftCtrl;
                    t.RightKey = Key.RightCtrl;
                    return true;
                }
                if (EqualsI(token, "shift"))
                {
                    t.Kind = TriggerKind.Keyboard;
                    t.IsAnySide = true;
                    t.LeftKey = Key.LeftShift;
                    t.RightKey = Key.RightShift;
                    return true;
                }
                if (EqualsI(token, "alt"))
                {
                    t.Kind = TriggerKind.Keyboard;
                    t.IsAnySide = true;
                    t.LeftKey = Key.LeftAlt;
                    t.RightKey = Key.RightAlt;
                    return true;
                }

                if (EqualsI(token, "leftctrl") || EqualsI(token, "lctrl"))
                    return MakeSingleKey(Key.LeftCtrl, out t);
                if (EqualsI(token, "rightctrl") || EqualsI(token, "rctrl"))
                    return MakeSingleKey(Key.RightCtrl, out t);

                if (EqualsI(token, "leftshift") || EqualsI(token, "lshift"))
                    return MakeSingleKey(Key.LeftShift, out t);
                if (EqualsI(token, "rightshift") || EqualsI(token, "rshift"))
                    return MakeSingleKey(Key.RightShift, out t);

                if (EqualsI(token, "leftalt") || EqualsI(token, "lalt"))
                    return MakeSingleKey(Key.LeftAlt, out t);
                if (EqualsI(token, "rightalt") || EqualsI(token, "ralt"))
                    return MakeSingleKey(Key.RightAlt, out t);

                return false;
            }

            public static bool MakeSingleKey(Key key, out Trigger t)
            {
                t = new Trigger
                {
                    Kind = TriggerKind.Keyboard,
                    Key = key,
                    IsAnySide = false
                };
                return true;
            }

            public static bool TryParseGamepad(string token, out GamepadButton btn)
            {
                btn = default;

                string s = token.Trim();
                if (s.StartsWith("Gamepad", StringComparison.OrdinalIgnoreCase))
                    s = s.Substring("Gamepad".Length);

                if (EqualsI(s, "A") || EqualsI(s, "South") || EqualsI(s, "Cross")) { btn = GamepadButton.South; return true; }
                if (EqualsI(s, "B") || EqualsI(s, "East") || EqualsI(s, "Circle")) { btn = GamepadButton.East; return true; }
                if (EqualsI(s, "X") || EqualsI(s, "West") || EqualsI(s, "Square")) { btn = GamepadButton.West; return true; }
                if (EqualsI(s, "Y") || EqualsI(s, "North") || EqualsI(s, "Triangle")) { btn = GamepadButton.North; return true; }

                if (EqualsI(s, "LB") || EqualsI(s, "LeftShoulder")) { btn = GamepadButton.LeftShoulder; return true; }
                if (EqualsI(s, "RB") || EqualsI(s, "RightShoulder")) { btn = GamepadButton.RightShoulder; return true; }

                if (EqualsI(s, "Back") || EqualsI(s, "Select")) { btn = GamepadButton.Select; return true; }
                if (EqualsI(s, "Start")) { btn = GamepadButton.Start; return true; }

                if (EqualsI(s, "LS") || EqualsI(s, "LeftStick")) { btn = GamepadButton.LeftStick; return true; }
                if (EqualsI(s, "RS") || EqualsI(s, "RightStick")) { btn = GamepadButton.RightStick; return true; }

                if (EqualsI(s, "DpadUp")) { btn = GamepadButton.DpadUp; return true; }
                if (EqualsI(s, "DpadDown")) { btn = GamepadButton.DpadDown; return true; }
                if (EqualsI(s, "DpadLeft")) { btn = GamepadButton.DpadLeft; return true; }
                if (EqualsI(s, "DpadRight")) { btn = GamepadButton.DpadRight; return true; }

                GamepadButton parsed;
                if (Enum.TryParse(s, true, out parsed))
                {
                    btn = parsed;
                    return true;
                }

                return false;
            }

            public static bool EqualsI(string a, string b)
            {
                return string.Equals(a, b, StringComparison.OrdinalIgnoreCase);
            }
        }

        private enum TriggerKind
        {
            Keyboard,
            Gamepad
        }
    }
}
