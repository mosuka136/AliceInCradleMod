using BetterExperience.HProvider;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.HotkeyManager
{
    /// <summary>
    /// 热键触发条件的统一接口。
    /// 实现类封装键盘键、键盘修饰键或手柄按钮，使 <see cref="HotkeyChord"/> 可以用同一流程判断输入状态。
    /// </summary>
    public interface IHotkeyTrigger
    {
        UnityProvider UnityService { get; }

        bool IsPressed();
        bool WasPressedThisFrame();
        string ToString();
        IHotkeyTrigger Clone();
    }

    /// <summary>
    /// 单个键盘按键触发器。
    /// </summary>
    public class KeyboardTrigger : IHotkeyTrigger
    {
        public UnityProvider UnityService { get; }
        public Key Key { get; set; } = Key.None;

        public KeyboardTrigger(UnityProvider unityService)
        {
            UnityService = unityService;
        }

        public KeyboardTrigger(Key key, UnityProvider unityService)
        {
            UnityService = unityService;
            Key = key;
        }

        public bool IsPressed()
        {
            var kb = UnityService?.KeyboardCurrent;
            if (kb == null)
                return false;
            return kb[Key].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var kb = UnityService?.KeyboardCurrent;
            if (kb == null)
                return false;
            return kb[Key].wasPressedThisFrame;
        }

        public static HotkeyResult<KeyboardTrigger> TryParse(string token, UnityProvider unityService)
        {
            if (string.IsNullOrWhiteSpace(token))
                return HotkeyResult<KeyboardTrigger>.Fail("Token is null or whitespace.");

            token = token.Trim();

            if (Enum.TryParse<Key>(token, true, out var key))
            {
                if (key == Key.None)
                    return HotkeyResult<KeyboardTrigger>.Fail("Parsed key is None.");

                return new KeyboardTrigger(key, unityService);
            }

            return HotkeyResult<KeyboardTrigger>.Fail($"Failed to parse token '{token}' as a Key.");
        }

        public override string ToString()
        {
            if (Key == Key.None)
                return string.Empty;
            return Key.ToString();
        }

        public IHotkeyTrigger Clone()
        {
            return new KeyboardTrigger(Key, UnityService);
        }
    }

    /// <summary>
    /// 键盘修饰键触发器。
    /// 支持左/右侧精确匹配，也支持不区分左右的 Ctrl、Shift、Alt 语义。
    /// </summary>
    public class KeyboardModifierTrigger : IHotkeyTrigger
    {
        public UnityProvider UnityService { get; }
        public Key LeftKey { get; set; } = Key.None;
        public Key RightKey { get; set; } = Key.None;
        /// <summary>
        /// 为 <c>true</c> 时左右任意一侧按下都满足条件。
        /// </summary>
        public bool IsAnySide { get; set; } = true;
        /// <summary>
        /// 当 <see cref="IsAnySide"/> 为 <c>false</c> 时，决定匹配左侧还是右侧按键。
        /// </summary>
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

        public static readonly KeyboardModifierTrigger Ctrl = new KeyboardModifierTrigger(Key.LeftCtrl, Key.RightCtrl, true, true, null);
        public static readonly KeyboardModifierTrigger Shift = new KeyboardModifierTrigger(Key.LeftShift, Key.RightShift, true, true, null);
        public static readonly KeyboardModifierTrigger Alt = new KeyboardModifierTrigger(Key.LeftAlt, Key.RightAlt, true, true, null);

        public KeyboardModifierTrigger(UnityProvider unityService)
        {
            UnityService = unityService;
        }

        public KeyboardModifierTrigger(Key key, UnityProvider unityService)
        {
            if (key == Key.LeftCtrl)
            {
                Ctrl.CopyModifiersTo(this);
                IsLeftSide = true;
            }
            else if (key == Key.RightCtrl)
            {
                Ctrl.CopyModifiersTo(this);
                IsLeftSide = false;
            }
            else if (key == Key.LeftShift)
            {
                Shift.CopyModifiersTo(this);
                IsLeftSide = true;
            }
            else if (key == Key.RightShift)
            {
                Shift.CopyModifiersTo(this);
                IsLeftSide = false;
            }
            else if (key == Key.LeftAlt)
            {
                Alt.CopyModifiersTo(this);
                IsLeftSide = true;
            }
            else if (key == Key.RightAlt)
            {
                Alt.CopyModifiersTo(this);
                IsLeftSide = false;
            }
            else
            {
                LeftKey = key;
                IsLeftSide = true;
            }

            UnityService = unityService;
            IsAnySide = false;
        }

        public KeyboardModifierTrigger(Key leftKey, Key rightKey, bool isAnySide, bool isLeftSide, UnityProvider unityService)
        {
            UnityService = unityService;

            LeftKey = leftKey;
            RightKey = rightKey;
            IsAnySide = isAnySide;
            IsLeftSide = isLeftSide;
        }

        public bool IsPressed()
        {
            var kb = UnityService?.KeyboardCurrent;
            if (kb == null)
                return false;

            if (IsAnySide)
                return kb[LeftKey].isPressed || kb[RightKey].isPressed;

            return IsLeftSide ? kb[LeftKey].isPressed : kb[RightKey].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var kb = UnityService?.KeyboardCurrent;
            if (kb == null)
                return false;

            if (IsAnySide)
                return kb[LeftKey].wasPressedThisFrame || kb[RightKey].wasPressedThisFrame;

            return IsLeftSide ? kb[LeftKey].wasPressedThisFrame : kb[RightKey].wasPressedThisFrame;
        }

        public static HotkeyResult<KeyboardModifierTrigger> TryParse(string token, UnityProvider unityService)
        {
            if (string.IsNullOrWhiteSpace(token))
                return HotkeyResult<KeyboardModifierTrigger>.Fail("Token is null or whitespace.");

            token = token.Trim();

            var result = new KeyboardModifierTrigger(unityService);

            if (EqualsL(token, CtrlStr))
            {
                Ctrl.CopyModifiersTo(result);
                return result;
            }

            if (EqualsL(token, ShiftStr))
            {
                Shift.CopyModifiersTo(result);
                return result;
            }

            if (EqualsL(token, AltStr))
            {
                Alt.CopyModifiersTo(result);
                return result;
            }

            if (EqualsL(token, LCtrlStr))
            {
                Ctrl.CopyModifiersTo(result);
                result.IsAnySide = false;
                result.IsLeftSide = true;
                return result;
            }

            if (EqualsL(token, RCtrlStr))
            {
                Ctrl.CopyModifiersTo(result);
                result.IsAnySide = false;
                result.IsLeftSide = false;
                return result;
            }

            if (EqualsL(token, LShiftStr))
            {
                Shift.CopyModifiersTo(result);
                result.IsAnySide = false;
                result.IsLeftSide = true;
                return result;
            }

            if (EqualsL(token, RShiftStr))
            {
                Shift.CopyModifiersTo(result);
                result.IsAnySide = false;
                result.IsLeftSide = false;
                return result;
            }

            if (EqualsL(token, LAltStr))
            {
                Alt.CopyModifiersTo(result);
                result.IsAnySide = false;
                result.IsLeftSide = true;
                return result;
            }

            if (EqualsL(token, RAltStr))
            {
                Alt.CopyModifiersTo(result);
                result.IsAnySide = false;
                result.IsLeftSide = false;
                return result;
            }

            return HotkeyResult<KeyboardModifierTrigger>.Fail("Unrecognized token.");
        }

        /// <summary>
        /// 复制修饰键定义，不复制 Unity 输入服务引用。
        /// 该方法用于静态模板向运行时实例传递左右键约定。
        /// </summary>
        public void CopyModifiersTo(KeyboardModifierTrigger other)
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

        public IHotkeyTrigger Clone()
        {
            return new KeyboardModifierTrigger(LeftKey, RightKey, IsAnySide, IsLeftSide, UnityService);
        }
    }

    /// <summary>
    /// 手柄按钮触发器。
    /// 配置文本允许省略 <c>Gamepad</c> 前缀解析，但写出时始终带前缀，以避免和键盘键名混淆。
    /// </summary>
    public class GamepadTrigger : IHotkeyTrigger
    {
        public UnityProvider UnityService { get; }
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

        public GamepadTrigger(UnityProvider unityService)
        {
            UnityService = unityService;
        }

        public GamepadTrigger(GamepadButton button, UnityProvider unityService)
        {
            UnityService = unityService;
            Button = button;
        }

        public bool IsPressed()
        {
            var gp = UnityService?.GamepadCurrent;
            if (gp == null)
                return false;
            return gp[Button].isPressed;
        }

        public bool WasPressedThisFrame()
        {
            var gp = UnityService?.GamepadCurrent;
            if (gp == null)
                return false;
            return gp[Button].wasPressedThisFrame;
        }

        public static HotkeyResult<GamepadTrigger> TryParse(string token, UnityProvider unityProvider)
        {
            if (string.IsNullOrWhiteSpace(token))
                return HotkeyResult<GamepadTrigger>.Fail("Token is null or whitespace.");

            string s = token.Trim();
            if (s.StartsWith(Prefix, StringComparison.OrdinalIgnoreCase))
                s = s.Substring(Prefix.Length);

            var result = new GamepadTrigger(unityProvider);

            if (EqualsL(s, SouthStr)) { result.Button = GamepadButton.South; return result; }
            if (EqualsL(s, EastStr)) { result.Button = GamepadButton.East; return result; }
            if (EqualsL(s, WestStr)) { result.Button = GamepadButton.West; return result; }
            if (EqualsL(s, NorthStr)) { result.Button = GamepadButton.North; return result; }

            if (EqualsL(s, LeftShoulderStr)) { result.Button = GamepadButton.LeftShoulder; return result; }
            if (EqualsL(s, RightShoulderStr)) { result.Button = GamepadButton.RightShoulder; return result; }

            if (EqualsL(s, SelectStr)) { result.Button = GamepadButton.Select; return result; }
            if (EqualsL(s, StartStr)) { result.Button = GamepadButton.Start; return result; }

            if (EqualsL(s, LeftStickStr)) { result.Button = GamepadButton.LeftStick; return result; }
            if (EqualsL(s, RightStickStr)) { result.Button = GamepadButton.RightStick; return result; }

            if (EqualsL(s, DpadUpStr)) { result.Button = GamepadButton.DpadUp; return result; }
            if (EqualsL(s, DpadDownStr)) { result.Button = GamepadButton.DpadDown; return result; }
            if (EqualsL(s, DpadLeftStr)) { result.Button = GamepadButton.DpadLeft; return result; }
            if (EqualsL(s, DpadRightStr)) { result.Button = GamepadButton.DpadRight; return result; }

            if (Enum.TryParse<GamepadButton>(s, true, out var parsed))
            {
                result.Button = parsed;
                return result;
            }

            return HotkeyResult<GamepadTrigger>.Fail("Failed to parse token.");
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

        public IHotkeyTrigger Clone()
        {
            return new GamepadTrigger(Button, UnityService);
        }
    }
}
