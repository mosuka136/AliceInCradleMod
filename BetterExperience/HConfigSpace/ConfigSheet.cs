using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 运行时配置表集合。
    /// 该模型保存已绑定的强类型配置表，面向业务代码和 GUI 使用；文件级结构由 <see cref="ConfigFileSheet"/> 维护。
    /// </summary>
    public class ConfigSheet : IEnumerable<KeyValuePair<string, ConfigTable>>
    {
        /// <summary>
        /// 表集合，使用有序字典保持声明顺序，便于 GUI 和写出内容稳定。
        /// </summary>
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
