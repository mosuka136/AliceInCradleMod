using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BetterExperience.HotkeyManager
{
    public class HotkeyChord
    {
        public const char Separator = '+';

        public List<IHotkeyTrigger> Modifiers { get; set; }
        public IHotkeyTrigger MainKey { get; set; }

        public HotkeyChord()
        {
            Modifiers = new List<IHotkeyTrigger>();
        }

        public HotkeyChord(IHotkeyTrigger main, params IHotkeyTrigger[] modifiers)
        {
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

            var keyboardTrigger = new KeyboardTrigger();
            var keyboardModifierTrigger = new KeyboardModifierTrigger();
            var gamepadTrigger = new GamepadTrigger();

            if (keyboardTrigger.TryParse(parts.Last()))
            {
                MainKey = keyboardTrigger;
                Modifiers.Clear();
                for (int i = 0; i < parts.Count - 1; i++)
                {
                    if (!keyboardModifierTrigger.TryParse(parts[i]))
                        return false;
                    var modifier = new KeyboardModifierTrigger();
                    keyboardModifierTrigger.CopyTo(modifier);
                    Modifiers.Add(modifier);
                }
            }
            else if (gamepadTrigger.TryParse(parts.First()) && parts.Count == 1)
            {
                MainKey = gamepadTrigger;
            }
            else
                return false;

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
            sb.Append(MainKey.ToString());
            return sb.ToString();
        }
    }
}
