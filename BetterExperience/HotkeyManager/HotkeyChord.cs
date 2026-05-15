using BetterExperience.HProvider;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;

namespace BetterExperience.HotkeyManager
{
    /// <summary>
    /// 表示一个完整按键组合，由若干修饰键和一个主键组成。
    /// 键盘组合使用 <c>+</c> 分隔，最后一段必须是主键；手柄组合也复用同一格式。
    /// </summary>
    public class HotkeyChord
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
        /// <summary>
        /// 是否具备可触发的主键。没有主键的组合仅作为 GUI 录制过程中的临时状态。
        /// </summary>
        public bool IsValid => MainKey != null;

        public HotkeyChord(UnityProvider unityService)
        {
            UnityService = unityService;
            Modifiers = new List<IHotkeyTrigger>();
        }

        public HotkeyChord(UnityProvider unityService, IHotkeyTrigger main, params IHotkeyTrigger[] modifiers)
        {
            UnityService = unityService;
            MainKey = main;
            Modifiers = modifiers.ToList();
        }

        public bool WasPressedThisFrame()
        {
            foreach (var modifier in Modifiers)
            {
                if (!modifier.IsPressed())
                    return false;
            }

            return MainKey.WasPressedThisFrame();
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

        public void ClearModifiers()
        {
            Modifiers.Clear();
        }

        public void Clear()
        {
            Modifiers.Clear();
            MainKey = null;
        }

        public bool TryParse(string chordStr)
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
                return false;

            var keyboardTrigger = new KeyboardTrigger(UnityService);
            var keyboardModifierTrigger = new KeyboardModifierTrigger(UnityService);
            var gamepadTrigger = new GamepadTrigger(UnityService);

            // 先尝试键盘主键：键盘组合只允许键盘修饰键，避免混合输入设备产生不可预测的触发条件。
            if (keyboardTrigger.TryParse(parts.Last()))
            {
                MainKey = keyboardTrigger;
                Modifiers.Clear();
                for (int i = 0; i < parts.Count - 1; i++)
                {
                    if (!keyboardModifierTrigger.TryParse(parts[i]))
                    {
                        HLog.Debug($"Failed to parse keyboard modifier in hotkey chord: {parts[i]}");
                        return false;
                    }
                    var modifier = new KeyboardModifierTrigger(UnityService);
                    keyboardModifierTrigger.CopyTo(modifier);
                    Modifiers.Add(modifier);
                }

                return true;
            }

            // 如果主键不是键盘键，则按手柄组合解析，前置段也必须是手柄按钮。
            if (!gamepadTrigger.TryParse(parts.Last()))
            {
                HLog.Debug($"Failed to parse hotkey main key: {parts.Last()}");
                return false;
            }

            MainKey = gamepadTrigger;
            Modifiers.Clear();
            for (int i = 0; i < parts.Count - 1; i++)
            {
                var modifier = new GamepadTrigger(UnityService);
                if (!modifier.TryParse(parts[i]))
                {
                    HLog.Debug($"Failed to parse gamepad modifier in hotkey chord: {parts[i]}");
                    return false;
                }
                Modifiers.Add(modifier);
            }

            return true;
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
    }
}
