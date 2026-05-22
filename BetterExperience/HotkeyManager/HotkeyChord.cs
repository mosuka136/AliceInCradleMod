using BetterExperience.HProvider;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BetterExperience.HotkeyManager
{
    /// <summary>
    /// 热键组合键接口。提供热键组合键的基本功能，包括检查是否按下、是否在当前帧按下、清除组合键等。
    /// </summary>
    public interface IHotkeyChord
    {
        /// <summary>
        /// 当前组合是否有效。无效的组合在任何时候都不会触发。
        /// </summary>
        bool IsValid { get; }

        bool IsPressed();
        bool WasPressedThisFrame();
        void Clear();
        string ToString();
        IHotkeyChord Clone();
    }

    /// <summary>
    /// 键盘组合键。由一个主键和零个或多个修饰键组成，触发时要求所有修饰键保持按下状态，并在同一帧按下主键。
    /// 键与键之间的文本分隔符为 <see cref="Separator"/>。
    /// </summary>
    public class KeyboardChord : IHotkeyChord
    {
        /// <summary>
        /// 组合内部修饰键与主键的文本分隔符。
        /// </summary>
        public const char Separator = '+';

        public UnityProvider UnityService { get; }

        /// <summary>
        /// 触发主键前必须保持按下的修饰键集合。
        /// </summary>
        public List<IHotkeyTrigger> Modifiers { get; set; }
        /// <summary>
        /// 当前帧按下时触发该组合的主键。
        /// </summary>
        public IHotkeyTrigger MainKey { get; set; }
        public bool HasAnyKey => (MainKey != null) || Modifiers.Count > 0;
        public bool IsValid => MainKey != null;

        public KeyboardChord(UnityProvider unityService)
        {
            UnityService = unityService;
            Modifiers = new List<IHotkeyTrigger>();
        }

        public KeyboardChord(UnityProvider unityService, IHotkeyTrigger main, params IHotkeyTrigger[] modifiers)
        {
            UnityService = unityService;
            MainKey = main;
            Modifiers = modifiers.ToList();
        }

        public bool IsPressed()
        {
            foreach (var modifier in Modifiers)
            {
                if (!modifier.IsPressed())
                    return false;
            }

            return MainKey != null && MainKey.IsPressed();
        }

        public bool WasPressedThisFrame()
        {
            foreach (var modifier in Modifiers)
            {
                if (!modifier.IsPressed())
                    return false;
            }

            return MainKey != null && MainKey.WasPressedThisFrame();
        }

        /// <summary>
        /// 添加键盘修饰键。
        /// 如果左右两侧同类修饰键都被录入，会合并为不区分左右的修饰键，方便用户输入。
        /// </summary>
        public void AddModifier(Key key)
        {
            if (!KeyboardModifierTrigger.IsModifierKey(key))
                return;

            var anotherKey = GetAnotherModifierKey(key);
            var modifiers = Modifiers.OfType<KeyboardModifierTrigger>().ToList();

            var currentModifiers = modifiers.Where(modifier =>
            {
                if (modifier.IsAnySide)
                {
                    if (modifier.LeftKey == key || modifier.RightKey == key)
                        return true;
                }
                else
                {
                    if (modifier.IsLeftSide)
                    {
                        if (modifier.LeftKey == key)
                            return true;
                    }
                    else
                    {
                        if (modifier.RightKey == key)
                            return true;
                    }
                }
                return false;
            });

            if (currentModifiers.Any())
                return;

            var oppositeModifiers = modifiers.Where(modifier =>
                !modifier.IsAnySide &&
                ((modifier.IsLeftSide && modifier.RightKey == key && modifier.LeftKey == anotherKey) ||
                 (!modifier.IsLeftSide && modifier.LeftKey == key && modifier.RightKey == anotherKey)));

            if (oppositeModifiers.Any())
            {
                foreach (var modifier in oppositeModifiers)
                    modifier.IsAnySide = true;

                return;
            }

            Modifiers.Add(new KeyboardModifierTrigger(key, UnityService));
        }

        /// <summary>
        /// 设置主键。主键必须是非修饰键，如果传入的键是修饰键则会被忽略。
        /// </summary>
        public void SetMainKey(Key key)
        {
            if (KeyboardModifierTrigger.IsModifierKey(key))
                return;

            MainKey = new KeyboardTrigger(key, UnityService);
        }

        public void ClearModifiers()
        {
            Modifiers.Clear();
        }

        public void Clear()
        {
            Modifiers.Clear();
            MainKey = null;
        }

        public static HotkeyResult<KeyboardChord> TryParse(string chordStr, UnityProvider unityService)
        {
            var raw = chordStr.Split(Separator);
            var parts = new List<string>(raw.Length);
            foreach (var part in raw)
            {
                var str = part.Trim();
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                parts.Add(str);
            }
            if (parts.Count == 0)
                return HotkeyResult<KeyboardChord>.Fail("No valid parts found in chord string.");

            var result = new KeyboardChord(unityService);

            var keyboardTriggerResult = KeyboardTrigger.TryParse(parts.Last(), unityService);
            if (!keyboardTriggerResult.Success)
                return HotkeyResult<KeyboardChord>.Fail($"Failed to parse main key.", keyboardTriggerResult.Errors);
            result.MainKey = keyboardTriggerResult.Value;

            for (int i = 0; i < parts.Count - 1; i++)
            {
                var keyboardModifierTriggerResult = KeyboardModifierTrigger.TryParse(parts[i], unityService);
                if (!keyboardModifierTriggerResult.Success)
                    return HotkeyResult<KeyboardChord>.Fail($"Failed to parse keyboard modifier: {parts[i]}", keyboardModifierTriggerResult.Errors);
                result.Modifiers.Add(keyboardModifierTriggerResult.Value);
            }

            return result;
        }

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var modifier in Modifiers)
            {
                sb.Append(modifier.ToString());
                sb.Append(Separator);
            }

            if (MainKey != null)
                sb.Append(MainKey.ToString());
            else
            {
                if (sb.Length > 0)
                    sb.Remove(sb.Length - 1, 1);
            }

            return sb.ToString();
        }

        public static Key GetAnotherModifierKey(Key key)
        {
            switch (key)
            {
                case Key.LeftShift:
                    return Key.RightShift;
                case Key.RightShift:
                    return Key.LeftShift;
                case Key.LeftCtrl:
                    return Key.RightCtrl;
                case Key.RightCtrl:
                    return Key.LeftCtrl;
                case Key.LeftAlt:
                    return Key.RightAlt;
                case Key.RightAlt:
                    return Key.LeftAlt;
                default:
                    return key;
            }
        }

        public IHotkeyChord Clone()
        {
            var clone = new KeyboardChord(UnityService)
            {
                MainKey = MainKey?.Clone()
            };

            foreach (var modifier in Modifiers)
            {
                clone.Modifiers.Add(modifier.Clone());
            }

            return clone;
        }
    }

    /// <summary>
    /// 游戏手柄组合键。与键盘组合键不同，游戏手柄组合键没有主键和修饰键的区分，组合内的所有键都必须在同一帧被按下才会触发。
    /// 键与键之间的文本分隔符为 <see cref="Separator"/>。
    /// </summary>
    public class GamepadChord : IHotkeyChord
    {
        /// <summary>
        /// 组合内部键与键的文本分隔符。
        /// </summary>
        public const char Separator = '+';

        public UnityProvider UnityService { get; }
        public List<IHotkeyTrigger> Buttons { get; set; }
        public bool IsValid => Buttons.Count > 0;
        public int Count => Buttons.Count;

        public GamepadChord(UnityProvider unityService)
        {
            UnityService = unityService;
            Buttons = new List<IHotkeyTrigger>();
        }

        public GamepadChord(UnityProvider unityService, params IHotkeyTrigger[] keys)
        {
            UnityService = unityService;
            Buttons = keys.ToList();
        }

        public bool IsPressed()
        {
            if (Buttons.Count == 0)
                return false;

            return Buttons.All(b => b.IsPressed());
        }

        public bool WasPressedThisFrame()
        {
            if (Buttons.Count == 0)
                return false;

            return Buttons.Any(b => b.WasPressedThisFrame()) && Buttons.All(b => b.IsPressed() || b.WasPressedThisFrame());
        }

        public void Clear()
        {
            Buttons.Clear();
        }

        /// <summary>
        /// 添加游戏手柄键。由于游戏手柄键之间没有主键和修饰键的区分，因此只要组合内没有重复的键就可以添加。
        /// </summary>
        public void AddButton(GamepadButton button)
        {
            if (Buttons.OfType<GamepadTrigger>().Any(k => k.Button == button))
                return;

            Buttons.Add(new GamepadTrigger(button, UnityService));
        }

        public static HotkeyResult<GamepadChord> TryParse(string chordStr, UnityProvider unityService)
        {
            var raw = chordStr.Split(Separator);
            var parts = new List<string>(raw.Length);
            foreach (var part in raw)
            {
                var str = part.Trim();
                if (string.IsNullOrWhiteSpace(str))
                    continue;
                parts.Add(str);
            }
            if (parts.Count == 0)
                return HotkeyResult<GamepadChord>.Fail("Failed to parse gamepad chord: empty input");

            var result = new GamepadChord(unityService);
            foreach (var part in parts)
            {
                var gamepadTriggerResult = GamepadTrigger.TryParse(part, unityService);
                if (!gamepadTriggerResult.Success)
                    return HotkeyResult<GamepadChord>.Fail($"Failed to parse gamepad modifier: {part}", gamepadTriggerResult.Errors);
                result.Buttons.Add(gamepadTriggerResult.Value);
            }

            return result;
        }

        public override string ToString()
        {
            return string.Join(Separator.ToString(), Buttons.Select(k => k.ToString()));
        }

        public IHotkeyChord Clone()
        {
            var clone = new GamepadChord(UnityService);
            foreach (var key in Buttons)
            {
                clone.Buttons.Add(key.Clone());
            }
            return clone;
        }
    }

    /// <summary>
    /// 热键组合键。可以同时表示键盘组合键和游戏手柄组合键，具体类型由内部的 <see cref="Chord"/> 决定。
    /// </summary>
    public class HotkeyChord : IHotkeyChord
    {
        public IHotkeyChord Chord { get; set; }

        public UnityProvider UnityService { get; }

        public bool IsValid => Chord?.IsValid ?? false;

        public HotkeyChord(UnityProvider unityService)
        {
            UnityService = unityService;
        }

        public HotkeyChord(IHotkeyChord chord, UnityProvider unityService)
        {
            Chord = chord;
            UnityService = unityService;
        }

        public static HotkeyResult<HotkeyChord> TryParse(string chordStr, UnityProvider unityService)
        {
            var keyboardResult = KeyboardChord.TryParse(chordStr, unityService);
            var gamepadResult = GamepadChord.TryParse(chordStr, unityService);

            if (keyboardResult.Success && gamepadResult.Success)
                return HotkeyResult<HotkeyChord>.Fail("Failed to parse hotkey chord: input can be parsed as both keyboard and gamepad chord, which is ambiguous.");

            if (keyboardResult.Success)
                return new HotkeyChord(keyboardResult.Value, unityService);

            if (gamepadResult.Success)
                return new HotkeyChord(gamepadResult.Value, unityService);

            return HotkeyResult<HotkeyChord>.Fail("Failed to parse hotkey chord as either keyboard or gamepad chord.", keyboardResult.Errors, gamepadResult.Errors);
        }

        public void Clear()
        {
            Chord?.Clear();
        }

        public IHotkeyChord Clone()
        {
            return new HotkeyChord(Chord?.Clone(), UnityService);
        }

        public bool IsPressed()
        {
            return Chord?.IsPressed() ?? false;
        }

        public bool WasPressedThisFrame()
        {
            return Chord?.WasPressedThisFrame() ?? false;
        }

        public override string ToString()
        {
            return Chord?.ToString() ?? string.Empty;
        }
    }
}
