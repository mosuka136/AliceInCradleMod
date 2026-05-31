using BetterExperience.HConfigGUI.Bindings;
using System.Collections.Generic;
using System.Linq;

namespace BetterExperience.HConfigGUI
{
    public class GuiStateStore
    {
        public const string Separator = "+";
        public static string IsPopupOpenKey => "IsPopupOpen";
        public static string SelectedTableIndexKey => "SelectedTableIndex";
        public static string TableButtonWidthKey => "TableButtonWidth";
        public static string LeadingBlankWidthKey => "LeadingBlankWidth";
        public static string RearBlankWidthKey => "RearBlankWidth";

        private readonly Dictionary<string, string> _textState = new Dictionary<string, string>();
        private readonly Dictionary<string, int> _intState = new Dictionary<string, int>();
        private readonly Dictionary<string, float> _floatState = new Dictionary<string, float>();
        private readonly Dictionary<string, bool> _boolState = new Dictionary<string, bool>();

        public string GetKey(IEntryBinding entry, string suffix = "")
        {
            return entry.Key + Separator + suffix;
        }

        public string GetText(string key, string defaultValue = "")
        {
            if (_textState.TryGetValue(key, out var value))
                return value;
            _textState[key] = defaultValue;
            return defaultValue;
        }

        public void SetText(string key, string value)
        {
            _textState[key] = value;
        }

        public void DeleteText(string key)
        {
            if (_textState.ContainsKey(key))
                _textState.Remove(key);
        }

        public void DeleteText(IEntryBinding entry)
        {
            var textState = _textState.Where(kv => kv.Key.StartsWith(entry.Key)).Select(kv => kv.Key).ToList();
            foreach (var key in textState)
            {
                _textState.Remove(key);
            }
        }

        public bool GetBool(string key, bool defaultValue = false)
        {
            if (_boolState.TryGetValue(key, out var value))
                return value;
            _boolState[key] = defaultValue;
            return defaultValue;
        }

        public void SetBool(string key, bool value)
        {
            _boolState[key] = value;
        }

        public void DeleteBool(string key)
        {
            if (_boolState.ContainsKey(key))
                _boolState.Remove(key);
        }

        public void DeleteBool(IEntryBinding entry)
        {
            var boolState = _boolState.Where(kv => kv.Key.StartsWith(entry.Key)).Select(kv => kv.Key).ToList();
            foreach (var key in boolState)
            {
                _boolState.Remove(key);
            }
        }

        public int GetInt(string key, int defaultValue = 0)
        {
            if (_intState.TryGetValue(key, out var value))
                return value;
            _intState[key] = defaultValue;
            return defaultValue;
        }

        public void SetInt(string key, int value)
        {
            _intState[key] = value;
        }

        public void DeleteInt(string key)
        {
            if (_intState.ContainsKey(key))
                _intState.Remove(key);
        }

        public void DeleteInt(IEntryBinding entry)
        {
            var intState = _intState.Where(kv => kv.Key.StartsWith(entry.Key)).Select(kv => kv.Key).ToList();
            foreach (var key in intState)
            {
                _intState.Remove(key);
            }
        }

        public float GetFloat(string key, float defaultValue = 0f)
        {
            if (_floatState.TryGetValue(key, out var value))
                return value;
            _floatState[key] = defaultValue;
            return defaultValue;
        }

        public void SetFloat(string key, float value)
        {
            _floatState[key] = value;
        }

        public void DeleteFloat(string key)
        {
            if (_floatState.ContainsKey(key))
                _floatState.Remove(key);
        }

        public void DeleteFloat(IEntryBinding entry)
        {
            var floatState = _floatState.Where(kv => kv.Key.StartsWith(entry.Key)).Select(kv => kv.Key).ToList();
            foreach (var key in floatState)
            {
                _floatState.Remove(key);
            }
        }
    }
}