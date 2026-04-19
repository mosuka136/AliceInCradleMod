using BetterExperience.HTranslatorSpace;
using System;
using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.HConfigFileSpace
{
    public class ConfigTable : IEnumerable<IConfigEntry>
    {
        public string Key { get; set; }
        public Translator Name { get; set; }
        public Translator Description { get; set; }
        public List<IConfigEntry> Table { get; private set; }
        public ConfigFileTableModel FileTable { get; private set; }

        public ConfigTable(string key, ConfigFileTableModel table, Translator name, Translator description)
        {
            if (!ConfigFileTableModel.IsValidTableName(key))
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
