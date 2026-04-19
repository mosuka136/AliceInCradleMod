using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.HotkeyManager
{
    public class Hotkey
    {
        public const char Separator = ',';

        public List<HotkeyChord> Hotkeys { get; set; }

        public Hotkey()
        {
            Hotkeys = new List<HotkeyChord>();
        }

        public Hotkey(params HotkeyChord[] hotkeys)
        {
            Hotkeys = hotkeys.ToList();
        }

        public Hotkey(IHotkeyTrigger main, params IHotkeyTrigger[] modifiers)
        {
            var chord = new HotkeyChord(main, modifiers);
            Hotkeys = new List<HotkeyChord> { chord };
        }

        public bool WasPressedThisFrame()
        {
            foreach (var ch in Hotkeys)
            {
                if (ch.WasPressedThisFrame())
                {
                    return true;
                }
            }
            return false;
        }

        public bool TryParse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            var chordStrings = text.Split(Separator);
            foreach (var ch in chordStrings)
            {
                var chordStr = ch.Trim();
                if (string.IsNullOrWhiteSpace(chordStr))
                    continue;

                var chord = new HotkeyChord();
                if (!chord.TryParse(chordStr))
                    return false;
                Hotkeys.Add(chord);
            }

            return true;
        }

        public override string ToString()
        {
            return string.Join(Separator.ToString(), Hotkeys.Select(h => h.ToString()));
        }
    }
}
