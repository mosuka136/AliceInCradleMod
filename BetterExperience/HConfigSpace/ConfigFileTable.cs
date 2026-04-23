using BetterExperience.HTranslatorSpace;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BetterExperience.HConfigSpace
{
    public class ConfigFileTable
    {
        public Translator Name { get; set; }
        public Translator Description { get; set; }
        public string Key { get; set; }
        public OrderedDictionary Table { get; private set; } = new OrderedDictionary();

        public ConfigFileTable(string tableKey, Translator description)
        {
            if (IsValidTableName(tableKey))
                Key = tableKey;
            else
                throw new ArgumentException($"Invalid table name: {tableKey}", nameof(tableKey));
            Description = description;
        }

        public ConfigFileResult<ConfigFileTable> AddEntry(ConfigFileEntry entry)
        {
            if (entry == null)
                return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.EntryNotFound, "Entry cannot be null"));

            if (Table.Contains(entry.Key))
                return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyName, $"Duplicate key: {entry.Key}"));

            Table.Add(entry.Key, entry);
            return this;
        }

        public ConfigFileResult<ConfigFileEntry> GetEntry(string key)
        {
            if (Table.Contains(key))
                return (ConfigFileEntry)Table[key];
            return ConfigFileResult<ConfigFileEntry>.Fail(new ConfigFileError(ConfigFileErrorCode.EntryNotFound, $"Entry not found: {key}"));
        }

        public ConfigFileResult<string> EncodeName()
        {
            if (Name == null)
                return string.Empty;
            var list = new List<string>();
            foreach (var name in Name)
            {
                if (string.IsNullOrEmpty(name))
                    continue;
                list.Add(name);
            }
            return $"# Name: {string.Join(", ", list)}";
        }

        public ConfigFileResult<string> EncodeDescription()
        {
            if (Description == null)
                return string.Empty;
            var sb = new StringBuilder();
            foreach (var description in Description)
            {
                if (string.IsNullOrEmpty(description))
                    continue;
                var lines = description.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                foreach (var line in lines)
                    sb.AppendLine($"## {line}");
            }
            return sb.ToString().Trim();
        }

        public ConfigFileResult<string> EncodeTableHeader()
        {
            if (!IsValidTableName(Key))
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Invalid table name: {Key}"));
            return $"[{Key}]";
        }

        public ConfigFileResult<string> EncodeTable()
        {
            var nameResult = EncodeName();
            if (!nameResult.Success)
                return ConfigFileResult<string>.Fail(nameResult.Errors);

            var descriptionResult = EncodeDescription();
            if (!descriptionResult.Success)
                return ConfigFileResult<string>.Fail(descriptionResult.Errors);

            var tableHeaderResult = EncodeTableHeader();
            if (!tableHeaderResult.Success)
                return ConfigFileResult<string>.Fail(tableHeaderResult.Errors);

            var sb = new StringBuilder();
            var result = new ConfigFileResult<string>();

            if (nameResult.Value != string.Empty)
                sb.AppendLine(nameResult.Value);
            if (descriptionResult.Value != string.Empty)
                sb.AppendLine(descriptionResult.Value);
            sb.AppendLine(tableHeaderResult.Value);
            sb.AppendLine();

            foreach (var entry in Table.Values)
            {
                var entryResult = ((ConfigFileEntry)entry).EncodeEntry();
                if (entryResult.Success)
                    sb.AppendLine(entryResult.Value);
                else
                    result.AddError(entryResult.Errors);
                sb.AppendLine();
            }

            result.SetValue(sb.ToString().Trim());
            return result;
        }

        public static bool IsValidTableName(string tableKey)
        {
            if (string.IsNullOrWhiteSpace(tableKey))
                return false;
            if (tableKey.All(c => char.IsLetterOrDigit(c) || c == '_'))
                return true;
            return false;
        }

        public static ConfigFileResult<ConfigFileTable> Create(string tableName, Translator description)
        {
            if (!IsValidTableName(tableName))
                return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Invalid table name: {tableName}"));
            var table = new ConfigFileTable(tableName, description);
            return table;
        }

        public static ConfigFileResult<ConfigFileTable> DecodeTable(string[] content, ref int index)
        {
            var headerResult = DecodeTableHeader(content, ref index);
            if (!headerResult.Success)
                return ConfigFileResult<ConfigFileTable>.Fail(headerResult.Errors);

            var table = new ConfigFileTable(headerResult.Value.Key, headerResult.Value.Description);
            var result = new ConfigFileResult<ConfigFileTable>(table, true, null);

            for (var i = index; index < content.Length && !DecodeTableHeader(content, ref i).Success; i = index)
            {
                var entryResult = ConfigFileEntry.DecodeEntry(content, ref index);
                if (entryResult.Success)
                {
                    var entryAddResult = table.AddEntry(entryResult.Value);
                    if (!entryAddResult.Success)
                        result.AddError(entryAddResult.Errors);
                }
                else
                {
                    result.AddError(entryResult.Errors);
                    if (entryResult.Errors.Any(e => e.Code == ConfigFileErrorCode.EndOfContent))
                        break;
                }
            }

            result.SetValue(table);
            return result;
        }

        public static ConfigFileResult<ConfigFileTable> DecodeTableHeader(string[] content, ref int index)
        {
            for (; index < content.Length; index++)
            {
                var line = content[index].Trim();

                if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                    continue;

                if (line.StartsWith("[") && line.EndsWith("]"))
                {
                    index++;
                    var tableName = line.Substring(1, line.Length - 2);
                    var tableResult = Create(tableName, new Translator());
                    if (!tableResult.Success)
                        return ConfigFileResult<ConfigFileTable>.Fail(tableResult.Errors);
                    return tableResult;
                }

                index++;
                return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableHeader, $"Invalid table header: {line}"));
            }

            return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.EndOfContent, "No more content to process"));
        }
    }
}
