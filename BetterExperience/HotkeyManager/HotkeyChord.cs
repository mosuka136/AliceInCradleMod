using BetterExperience.HProvider;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine.InputSystem;

namespace BetterExperience.HotkeyManager
{
    public class HotkeyChord
    {
        public const char Separator = '+';

        public UnityProvider UnityService { get; }

        public List<IHotkeyTrigger> Modifiers { get; set; }
        public IHotkeyTrigger MainKey { get; set; }
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

            if (keyboardTrigger.TryParse(parts.Last()))
            {
                MainKey = keyboardTrigger;
                Modifiers.Clear();
                for (int i = 0; i < parts.Count - 1; i++)
                {
                    if (!keyboardModifierTrigger.TryParse(parts[i]))
                        return false;
                    var modifier = new KeyboardModifierTrigger(UnityService);
                    keyboardModifierTrigger.CopyTo(modifier);
                    Modifiers.Add(modifier);
                }

                return true;
            }

            if (!gamepadTrigger.TryParse(parts.Last()))
                return false;

            MainKey = gamepadTrigger;
            Modifiers.Clear();
            for (int i = 0; i < parts.Count - 1; i++)
            {
                var modifier = new GamepadTrigger(UnityService);
                if (!modifier.TryParse(parts[i]))
                    return false;
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
