using BetterExperience.HConfigSpace;
using BetterExperience.HTranslatorSpace;
using System;

namespace BetterExperience.HConfigGUI
{
    public class UiEntryModel
    {
        private IConfigEntry _entry;

        public Translator Name => _entry.Name;
        public Translator Description => _entry.Description;
        public Type ValueType => _entry.ValueType;
        public object Value
        {
            get => _entry.BoxedValue;
            set
            {
                if (value == null)
                    return;
                _entry.BoxedValue = value;
                CacheValue = null;
                CacheValueString = string.Empty;
            }
        }
        public object DefaultValue => _entry.BoxedDefaultValue;
        public object CacheValue { get; set; } = null;
        public string CacheValueString {  get; set; } = string.Empty;

        public IUiMetadata Metadata { get; }

        public UiEntryModel(IConfigEntry entry)
        {
            _entry = entry;
            Metadata = UiMetadataHelper.GetMetadata(entry);
            _entry.OnValueChangedBase += (s, e) =>
            {
                CacheValue = null;
                CacheValueString = string.Empty;
            };
        }
    }
}
