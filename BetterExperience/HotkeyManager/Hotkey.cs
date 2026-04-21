using BetterExperience.HConfigFileSpace;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.HotkeyManager
{
    public class Hotkey : IConfigEntryAdapter
    {
        public const char Separator = ',';

        public List<HotkeyChord> Hotkeys { get; set; }

        public int Count => Hotkeys.Count;

        public bool Valid { get; set; } = true;

        public Hotkey()
        {
            Hotkeys = new List<HotkeyChord>();
        }

        public Hotkey(string hotkey)
        {
            Hotkeys = new List<HotkeyChord>();
            if (!TryParse(hotkey))
                throw new ArgumentException($"Invalid hotkey string: {hotkey}");
        }

        public Hotkey(Hotkey hotkey)
        {
            Hotkeys = hotkey.Hotkeys.Select(h => new HotkeyChord(h.MainKey, h.Modifiers.ToArray())).ToList();
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
            if (!Valid)
                return false;

            foreach (var ch in Hotkeys)
            {
                if (ch.WasPressedThisFrame())
                {
                    return true;
                }
            }
            return false;
        }

        public void Add(HotkeyChord chord)
        {
            if (chord != null && !Hotkeys.Contains(chord))
                Hotkeys.Add(chord);
        }

        public void Remove(HotkeyChord chord)
        {
            if (chord != null)
                Hotkeys.Remove(chord);
        }

        public void RemoveInvalidHotkey()
        {
            Hotkeys.RemoveAll(h => !h.IsValid || h == null);
        }

        public bool HasSameHotkey(Hotkey other)
        {
            if (other == null)
                return false;

            var thisChords = new List<string>(Hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));
            var otherChords = new List<string>(other.Hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrWhiteSpace(s)));

            thisChords.Sort();
            otherChords.Sort();

            return thisChords.SequenceEqual(otherChords);
        }

        public bool TryParse(string text)
        {
            if (string.IsNullOrWhiteSpace(text))
                return false;

            Hotkeys.Clear();

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
            return string.Join(Separator.ToString(), Hotkeys.Select(h => h.ToString()).Where(s => !string.IsNullOrEmpty(s)));
        }

        public ConfigFileResult<string> Encode()
        {
            return ToString();
        }

        public ConfigFileResult<object> Decode(string content)
        {
            if (TryParse(content))
                return this;
            return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Failed to decode Hotkey"));
        }

        public ConfigFileResult<string> EncodeValueType()
        {
            return "Hotkey";
        }
    }
}
