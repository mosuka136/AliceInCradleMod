using BetterExperience.HAdapter;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.HotkeyManager
{
    public interface IHotkeyTrigger
    {
        bool IsPressed();
        bool WasPressedThisFrame();
        bool TryParse(string token);
        string ToString();
    }

    public class KeyboardTrigger : IHotkeyTrigger
    {
        public Key Key { get; set; } = Key.None;

        public KeyboardTrigger()
        {
            
        }

        public KeyboardTrigger(Key key)
        {
            Key = key;
        }

        public bool IsPressed()
        {
            var kb = UnityInputAdapter.KeyboardCurrent;
            if (kb == null)
                return false;
            return kb[Key].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var kb = UnityInputAdapter.KeyboardCurrent;
            if (kb == null)
                return false;
            return kb[Key].wasPressedThisFrame;
        }

        public bool TryParse(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            token = token.Trim();

            if (Enum.TryParse<Key>(token, true, out var key))
            {
                Key = key;
                return true;
            }
            return false;
        }

        public override string ToString()
        {
            if (Key == Key.None)
                return string.Empty;
            return Key.ToString();
        }
    }

    public class KeyboardModifierTrigger : IHotkeyTrigger
    {
        public Key LeftKey { get; set; } = Key.None;
        public Key RightKey { get; set; } = Key.None;
        public bool IsAnySide { get; set; } = true;
        public bool IsLeftSide { get; set; } = true;

        public static readonly List<string> CtrlStr = new List<string>() { "Ctrl", "Control" };
        public static readonly List<string> ShiftStr = new List<string>() { "Shift" };
        public static readonly List<string> AltStr = new List<string>() { "Alt" };
        public static readonly List<string> LCtrlStr = new List<string>() { "LeftCtrl", "LCtrl" };
        public static readonly List<string> RCtrlStr = new List<string>() { "RightCtrl", "RCtrl" };
        public static readonly List<string> LShiftStr = new List<string>() { "LeftShift", "LShift" };
        public static readonly List<string> RShiftStr = new List<string>() { "RightShift", "RShift" };
        public static readonly List<string> LAltStr = new List<string>() { "LeftAlt", "LAlt" };
        public static readonly List<string> RAltStr = new List<string>() { "RightAlt", "RAlt" };

        public static readonly KeyboardModifierTrigger Ctrl = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, true);
        public static readonly KeyboardModifierTrigger Shift = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, true, true);
        public static readonly KeyboardModifierTrigger Alt = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, true, true);

        public KeyboardModifierTrigger()
        {

        }

        public KeyboardModifierTrigger(Key key)
        {
            if (key == Key.LeftCtrl)
            {
                Ctrl.CopyTo(this);
                IsLeftSide = true;
            }
            else if (key == Key.RightCtrl)
            {
                Ctrl.CopyTo(this);
                IsLeftSide = false;
            }
            else if (key == Key.LeftShift)
            {
                Shift.CopyTo(this);
                IsLeftSide = true;
            }
            else if (key == Key.RightShift)
            {
                Shift.CopyTo(this);
                IsLeftSide = false;
            }
            else if (key == Key.LeftAlt)
            {
                Alt.CopyTo(this);
                IsLeftSide = true;
            }
            else if (key == Key.RightAlt)
            {
                Alt.CopyTo(this);
                IsLeftSide = false;
            }
            else
            {
                LeftKey = key;
                IsLeftSide = true;
            }

            IsAnySide = false;
        }

        public KeyboardModifierTrigger(Key leftKey, Key rightKey, bool isAnySide, bool isLeftSide)
        {
            LeftKey = leftKey;
            RightKey = rightKey;
            IsAnySide = isAnySide;
            IsLeftSide = isLeftSide;
        }

        public bool IsPressed()
        {
            var kb = UnityInputAdapter.KeyboardCurrent;
            if (kb == null)
                return false;

            if (IsAnySide)
                return kb[LeftKey].isPressed || kb[RightKey].isPressed;

            return IsLeftSide ? kb[LeftKey].isPressed : kb[RightKey].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var kb = UnityInputAdapter.KeyboardCurrent;
            if (kb == null)
                return false;

            if (IsAnySide)
                return kb[LeftKey].wasPressedThisFrame || kb[RightKey].wasPressedThisFrame;

            return IsLeftSide ? kb[LeftKey].wasPressedThisFrame : kb[RightKey].wasPressedThisFrame;
        }

        public bool TryParse(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            token = token.Trim();

            if (EqualsL(token, CtrlStr))
            {
                Ctrl.CopyTo(this);
                return true;
            }

            if (EqualsL(token, ShiftStr))
            {
                Shift.CopyTo(this);
                return true;
            }

            if (EqualsL(token, AltStr))
            {
                Alt.CopyTo(this);
                return true;
            }

            if (EqualsL(token, LCtrlStr))
            {
                Ctrl.CopyTo(this);
                IsAnySide = false;
                IsLeftSide = true;
                return true;
            }

            if (EqualsL(token, RCtrlStr))
            {
                Ctrl.CopyTo(this);
                IsAnySide = false;
                IsLeftSide = false;
                return true;
            }

            if (EqualsL(token, LShiftStr))
            {
                Shift.CopyTo(this);
                IsAnySide = false;
                IsLeftSide = true;
                return true;
            }

            if (EqualsL(token, RShiftStr))
            {
                Shift.CopyTo(this);
                IsAnySide = false;
                IsLeftSide = false;
                return true;
            }

            if (EqualsL(token, LAltStr))
            {
                Alt.CopyTo(this);
                IsAnySide = false;
                IsLeftSide = true;
                return true;
            }

            if (EqualsL(token, RAltStr))
            {
                Alt.CopyTo(this);
                IsAnySide = false;
                IsLeftSide = false;
                return true;
            }

            return false;
        }

        public void CopyTo(KeyboardModifierTrigger other)
        {
            other.LeftKey = LeftKey;
            other.RightKey = RightKey;
            other.IsAnySide = IsAnySide;
            other.IsLeftSide = IsLeftSide;
        }

        public override string ToString()
        {
            if (IsAnySide)
            {
                if (LeftKey == Key.None || RightKey == Key.None)
                    return string.Empty;
            }
            else
            {
                if (IsLeftSide)
                {
                    if (LeftKey == Key.None)
                        return string.Empty;
                }
                else
                {
                    if (RightKey == Key.None)
                        return string.Empty;
                }
            }

            if (LeftKey == Key.LeftCtrl && RightKey == Key.RightCtrl)
            {
                if (IsAnySide)
                    return CtrlStr.FirstOrDefault();
                else
                    return IsLeftSide ? LCtrlStr.FirstOrDefault() : RCtrlStr.FirstOrDefault();
            }

            if (LeftKey == Key.LeftShift && RightKey == Key.RightShift)
            {
                if (IsAnySide)
                    return ShiftStr.FirstOrDefault();
                else
                    return IsLeftSide ? LShiftStr.FirstOrDefault() : RShiftStr.FirstOrDefault();
            }

            if (LeftKey == Key.LeftAlt && RightKey == Key.RightAlt)
            {
                if (IsAnySide)
                    return AltStr.FirstOrDefault();
                else
                    return IsLeftSide ? LAltStr.FirstOrDefault() : RAltStr.FirstOrDefault();
            }

            return IsLeftSide ? LeftKey.ToString() : RightKey.ToString();
        }

        public static bool EqualsL(string entry, List<string> list)
        {
            foreach (var item in list)
            {
                if (item.Equals(entry, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public static bool IsModifierKey(Key key)
        {
            return key == Key.LeftCtrl || key == Key.RightCtrl || key == Key.LeftShift || key == Key.RightShift || key == Key.LeftAlt || key == Key.RightAlt;
        }
    }

    public class GamepadTrigger : IHotkeyTrigger
    {
        public GamepadButton Button { get; set; } = GamepadButton.A;

        public const string Prefix = "Gamepad";

        public static readonly List<string> SouthStr = new List<string>() { "A", "South", "Cross" };
        public static readonly List<string> EastStr = new List<string>() { "B", "East", "Circle" };
        public static readonly List<string> WestStr = new List<string>() { "X", "West", "Square" };
        public static readonly List<string> NorthStr = new List<string>() { "Y", "North", "Triangle" };
        public static readonly List<string> LeftShoulderStr = new List<string>() { "LB", "LeftShoulder" };
        public static readonly List<string> RightShoulderStr = new List<string>() { "RB", "RightShoulder" };
        public static readonly List<string> SelectStr = new List<string>() { "Back", "Select" };
        public static readonly List<string> StartStr = new List<string>() { "Start" };
        public static readonly List<string> LeftStickStr = new List<string>() { "LS", "LeftStick" };
        public static readonly List<string> RightStickStr = new List<string>() { "RS", "RightStick" };
        public static readonly List<string> DpadUpStr = new List<string>() { "DpadUp" };
        public static readonly List<string> DpadDownStr = new List<string>() { "DpadDown" };
        public static readonly List<string> DpadLeftStr = new List<string>() { "DpadLeft" };
        public static readonly List<string> DpadRightStr = new List<string>() { "DpadRight" };

        public GamepadTrigger()
        {
            
        }

        public GamepadTrigger(GamepadButton button)
        {
            Button = button;
        }

        public bool IsPressed()
        {
            var gp = UnityInputAdapter.GamepadCurrent;
            if (gp == null)
                return false;
            return gp[Button].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var gp = UnityInputAdapter.GamepadCurrent;
            if (gp == null)
                return false;
            return gp[Button].wasPressedThisFrame;
        }

        public bool TryParse(string token)
        {
            if (string.IsNullOrWhiteSpace(token))
                return false;

            string s = token.Trim();
            if (s.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                s = s.Substring(Prefix.Length);

            if (EqualsL(s, SouthStr)) { Button = GamepadButton.South; return true; }
            if (EqualsL(s, EastStr)) { Button = GamepadButton.East; return true; }
            if (EqualsL(s, WestStr)) { Button = GamepadButton.West; return true; }
            if (EqualsL(s, NorthStr)) { Button = GamepadButton.North; return true; }

            if (EqualsL(s, LeftShoulderStr)) { Button = GamepadButton.LeftShoulder; return true; }
            if (EqualsL(s, RightShoulderStr)) { Button = GamepadButton.RightShoulder; return true; }

            if (EqualsL(s, SelectStr)) { Button = GamepadButton.Select; return true; }
            if (EqualsL(s, StartStr)) { Button = GamepadButton.Start; return true; }

            if (EqualsL(s, LeftStickStr)) { Button = GamepadButton.LeftStick; return true; }
            if (EqualsL(s, RightStickStr)) { Button = GamepadButton.RightStick; return true; }

            if (EqualsL(s, DpadUpStr)) { Button = GamepadButton.DpadUp; return true; }
            if (EqualsL(s, DpadDownStr)) { Button = GamepadButton.DpadDown; return true; }
            if (EqualsL(s, DpadLeftStr)) { Button = GamepadButton.DpadLeft; return true; }
            if (EqualsL(s, DpadRightStr)) { Button = GamepadButton.DpadRight; return true; }

            if (Enum.TryParse<GamepadButton>(s, true, out var parsed))
            {
                Button = parsed;
                return true;
            }

            return false;
        }

        public static bool EqualsL(string entry, List<string> list)
        {
            foreach (var item in list)
            {
                if (item.Equals(entry, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }

        public override string ToString()
        {
            switch(Button)
            {
                case GamepadButton.South: return Prefix + SouthStr.FirstOrDefault();
                case GamepadButton.North: return Prefix + NorthStr.FirstOrDefault();
                case GamepadButton.West: return Prefix + WestStr.FirstOrDefault();
                case GamepadButton.East: return Prefix + EastStr.FirstOrDefault();
                case GamepadButton.LeftShoulder: return Prefix + LeftShoulderStr.FirstOrDefault();
                case GamepadButton.RightShoulder: return Prefix + RightShoulderStr.FirstOrDefault();
                case GamepadButton.Select: return Prefix + SelectStr.FirstOrDefault();
                case GamepadButton.Start: return Prefix + StartStr.FirstOrDefault();
                case GamepadButton.LeftStick: return Prefix + LeftStickStr.FirstOrDefault();
                case GamepadButton.RightStick: return Prefix + RightStickStr.FirstOrDefault();
                case GamepadButton.DpadUp: return Prefix + DpadUpStr.FirstOrDefault();
                case GamepadButton.DpadDown: return Prefix + DpadDownStr.FirstOrDefault();
                case GamepadButton.DpadLeft: return Prefix + DpadLeftStr.FirstOrDefault();
                case GamepadButton.DpadRight: return Prefix + DpadRightStr.FirstOrDefault();
                default: return Prefix + Button.ToString();
            }
        }
    }
}
