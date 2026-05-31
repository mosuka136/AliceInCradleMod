using BetterExperience.HConfigGUI.Bindings;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI
{
    public class EntryChangeSink
    {
        private readonly Dictionary<IEntryBinding, (object value, float delay)> _values = new Dictionary<IEntryBinding, (object value, float delay)>();

        public void SetValue(IEntryBinding entry, object newValue, float delay = 0.0f)
        {
            if (entry == null)
                return;

            if (delay <= 0.0f)
            {
                if (Equals(entry.Value, newValue))
                    return;
                entry.Value = newValue;
                GuiPipe.InvokeOnEntryValueChanged(entry);
            }
            else
                _values[entry] = (newValue, delay);
        }

        public void ResetValue(IEntryBinding entry)
        {
            if (entry == null)
                return;
            
            if (_values.ContainsKey(entry))
                _values.Remove(entry);

            entry.ResetValue();
            GuiPipe.InvokeOnEntryValueReset(entry);
        }

        public void FlushValue(float deltaTime)
        {
            var keys = new List<IEntryBinding>(_values.Keys);
            foreach (var key in keys)
            {
                var (value, delay) = _values[key];
                delay -= deltaTime;
                if (delay <= 0.0f)
                {
                    _values.Remove(key);
                    if (Equals(key.Value, value))
                        continue;
                    key.Value = value;
                    GuiPipe.InvokeOnEntryValueChanged(key);
                }
                else
                    _values[key] = (value, delay);
            }
        }
    }
}