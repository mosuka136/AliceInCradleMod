using System;
using System.Collections.Generic;

namespace BetterExperience.ConfigFileSpace
{
    public class ConfigTable
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IConfigEntry> Table { get; private set; }

        public ConfigTable(string name, string description)
        {
            if (!ConfigFileTablesModel.Table.IsValidTableName(name))
                throw new ArgumentException($"Invalid config table name: {name}.", nameof(name));
            Name = name;
            Description = description ?? string.Empty;
            Table = new List<IConfigEntry>();
        }
    }
}
