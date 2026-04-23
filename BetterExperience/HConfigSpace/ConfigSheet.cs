using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BetterExperience.HConfigSpace
{
    public class ConfigSheet : IEnumerable<KeyValuePair<string, ConfigTable>>
    {
        public OrderedDictionary Sheet { get; private set; }

        public IEnumerable<string> Keys
        {
            get
            {
                foreach (DictionaryEntry entry in Sheet)
                {
                    yield return (string)entry.Key;
                }
            }
        }

        public IEnumerable<ConfigTable> Values
        {
            get
            {
                foreach (DictionaryEntry entry in Sheet)
                {
                    yield return (ConfigTable)entry.Value;
                }
            }
        }

        public int Count => Sheet.Count;

        public ConfigSheet()
        {
            Sheet = new OrderedDictionary();
        }

        public void Add(string tableKey, ConfigTable table)
        {
            Sheet.Add(tableKey, table);
        }

        public bool Contains(string tableKey)
        {
            return Sheet.Contains(tableKey);
        }

        public ConfigTable this[string key]
        {
            get
            {
                return (ConfigTable)Sheet[key];
            }
        }

        public IEnumerator<KeyValuePair<string, ConfigTable>> GetEnumerator()
        {
            foreach (DictionaryEntry entry in Sheet)
            {
                yield return new KeyValuePair<string, ConfigTable>((string)entry.Key, (ConfigTable)entry.Value);
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
