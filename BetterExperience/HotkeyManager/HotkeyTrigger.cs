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
        public Key Key { get; set; }

        public KeyboardTrigger()
        {
            
        }

        public KeyboardTrigger(Key key)
        {
            Key = key;
        }

        public bool IsPressed()
        {
            var kb = Keyboard.current;
            if (kb == null)
                return false;
            return kb[Key].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var kb = Keyboard.current;
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
            return Key.ToString();
        }
    }

    public class  KeyboardModifierTrigger: IHotkeyTrigger
    {
        public Key LeftKey {  get; set; }
        public Key RightKey { get; set; }
        public bool IsAnySide { get; set; }
        public bool IsLeftSide { get; set; }

        public static readonly List<string> CtrlStr = new List<string>() { "ctrl", "control" };
        public static readonly List<string> ShiftStr = new List<string>() { "shift" };
        public static readonly List<string> AltStr = new List<string>() { "alt" };
        public static readonly List<string> LCtrlStr = new List<string>() { "leftctrl", "lctrl" };
        public static readonly List<string> RCtrlStr = new List<string>() { "rightctrl", "rctrl" };
        public static readonly List<string> LShiftStr = new List<string>() { "leftshift", "lshift" };
        public static readonly List<string> RShiftStr = new List<string>() { "rightshift", "rshift" };
        public static readonly List<string> LAltStr = new List<string>() { "leftalt", "lalt" };
        public static readonly List<string> RAltStr = new List<string>() { "rightalt", "ralt" };

        public static readonly KeyboardModifierTrigger Ctrl = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, true);
        public static readonly KeyboardModifierTrigger Shift = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, true, true);
        public static readonly KeyboardModifierTrigger Alt = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, true, true);

        public KeyboardModifierTrigger()
        {
            
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
            var kb = Keyboard.current;
            if (kb == null)
                return false;

            if (IsAnySide)
                return kb[LeftKey].isPressed || kb[RightKey].isPressed;

            return IsLeftSide ? kb[LeftKey].isPressed : kb[RightKey].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var kb = Keyboard.current;
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

        public static bool EqualsL(string entry, List<string> list)
        {
            foreach (var item in list)
            {
                if (item.Equals(entry, StringComparison.OrdinalIgnoreCase))
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
    }

    public class GamepadTrigger : IHotkeyTrigger
    {
        public GamepadButton Button { get; set; }

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
            var gp = Gamepad.current;
            if (gp == null)
                return false;
            return gp[Button].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var gp = Gamepad.current;
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
