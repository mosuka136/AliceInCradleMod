using BetterExperience.HTranslatorSpace;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 运行时配置表。
    /// 它把文件表 <see cref="ConfigFileTable"/> 与一组强类型配置项关联起来，供 GUI 按表展示和业务代码查找。
    /// </summary>
    public class ConfigTable : IEnumerable<IConfigEntry>
    {
        /// <summary>
        /// 表键名，对应配置文件中的表头。
        /// </summary>
        public string Key { get; set; }
        public Translator Name { get; set; }
        public Translator Description { get; set; }
        /// <summary>
        /// 表内已绑定配置项，顺序与声明顺序一致。
        /// </summary>
        public List<IConfigEntry> Table { get; private set; }
        /// <summary>
        /// 与该运行时表对应的文件表模型。
        /// </summary>
        public ConfigFileTable FileTable { get; private set; }

        public ConfigTable(string key, ConfigFileTable table, Translator name, Translator description)
        {
            if (!ConfigFileTable.IsValidTableName(key))
                throw new ArgumentException($"Invalid config table name: {key}.", nameof(key));
            Key = key;
            Name = name ?? new Translator(string.Empty);
            Description = description ?? new Translator(string.Empty);
            Table = new List<IConfigEntry>();

            table.Name = Name;
            table.Description = Description;
            FileTable = table;
        }

        public void Add(IConfigEntry entry)
        {
            if (entry == null)
                return;
            Table.Add(entry);
        }

        public IEnumerator<IConfigEntry> GetEnumerator()
        {
            return Table.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return Table.GetEnumerator();
        }
    }
}
