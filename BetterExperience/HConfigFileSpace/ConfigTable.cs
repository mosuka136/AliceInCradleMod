using BetterExperience.HTranslatorSpace;
using System;
using System.Collections.Generic;

namespace BetterExperience.HConfigFileSpace
{
    public class ConfigTable
    {
        public string Key { get; set; }
        public Translator Name { get; set; }
        public Translator Description { get; set; }
        public List<IConfigEntry> Table { get; private set; }

        public ConfigTable(string key, Translator name, Translator description)
        {
            if (!ConfigFileTablesModel.Table.IsValidTableName(key))
                throw new ArgumentException($"Invalid config table name: {key}.", nameof(key));
            Key = key;
            Name = name ?? new Translator(string.Empty);
            Description = description ?? new Translator(string.Empty);
            Table = new List<IConfigEntry>();
        }
    }
}
