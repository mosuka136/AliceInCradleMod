using System;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BetterExperience.ConfigFileSpace
{
    public class ConfigFileTablesModel
    {
        public OrderedDictionary Tables { get; private set; } = new OrderedDictionary();

        public ConfigFileResult<Table> AddTable(Table table)
        {
            return AddTable(table.TableName, table);
        }

        public ConfigFileResult<Table> AddTable(string tableName, Table table)
        {
            if (!Table.IsValidTableName(tableName))
                return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Invalid table name: {tableName}"));
            if (table == null)
                return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, "Table cannot be null"));
            if (Tables.Contains(tableName))
                return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Table name already exists: {tableName}"));
            Tables.Add(tableName, table);
            return table;
        }

        public ConfigFileResult<Table> GetTable(string tableName)
        {
            if (Tables.Contains(tableName))
                return (Table)Tables[tableName];
            return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, $"Table not found: {tableName}"));
        }

        public ConfigFileResult<ConfigFileEntryModel> GetEntry(string tableName, string key)
        {
            if (Tables.Contains(tableName))
            {
                var table = (Table)Tables[tableName];
                return table.GetEntry(key);
            }
            return ConfigFileResult<ConfigFileEntryModel>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, $"Table not found: {tableName}"));
        }

        public ConfigFileResult<string> EncodeTables()
        {
            var sb = new StringBuilder();
            var result = new ConfigFileResult<string>();
            foreach (Table table in Tables.Values)
            {
                var tableResult = table.EncodeTable();
                if (tableResult.Success)
                    sb.AppendLine(tableResult.Value);
                else
                    result.AddError(tableResult.Errors);
                sb.AppendLine();
            }

            result.SetValue(sb.ToString().Trim());
            return result;
        }

        public static ConfigFileResult<Table> CreateTable(string tableName, string description)
        {
            var table = Table.Create(tableName, description);
            if (!table.Success)
                return ConfigFileResult<Table>.Fail(table.Errors);
            return table.Value;
        }

        public static ConfigFileResult<ConfigFileTablesModel> DecodeTables(string[] content, ref int index)
        {
            var model = new ConfigFileTablesModel();
            var result = new ConfigFileResult<ConfigFileTablesModel>(model, true, null);

            while (index < content.Length)
            {
                var tableResult = Table.DecodeTable(content, ref index);
                if (tableResult.Success)
                {
                    var addTableResult = model.AddTable(tableResult.Value);
                    if (!addTableResult.Success)
                        result.AddError(addTableResult.Errors);
                }
                else
                {
                    if (tableResult.Errors.Any(e => e.Code == ConfigFileErrorCode.EndOfContent))
                        break;
                    result.AddError(tableResult.Errors);
                }
            }

            result.SetValue(model);
            return result;
        }

        public class Table
        {
            public string Description { get; set; }
            public string TableName { get; set; }
            public OrderedDictionary Entries { get; set; } = new OrderedDictionary();

            public Table(string tableName, string description)
            {
                if (IsValidTableName(tableName))
                    TableName = tableName;
                else
                    throw new ArgumentException($"Invalid table name: {tableName}", nameof(tableName));
                Description = description;
            }

            public Table(string tableName, string description, OrderedDictionary entries) : this(tableName, description)
            {
                Entries = entries ?? new OrderedDictionary();
            }

            public ConfigFileResult<Table> AddEntry(ConfigFileEntryModel entry)
            {
                if (entry == null)
                    return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.EntryNotFound, "Entry cannot be null"));

                if (Entries.Contains(entry.Key))
                    return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyName, $"Duplicate key: {entry.Key}"));

                Entries.Add(entry.Key, entry);
                return this;
            }

            public ConfigFileResult<ConfigFileEntryModel> GetEntry(string key)
            {
                if (Entries.Contains(key))
                    return (ConfigFileEntryModel)Entries[key];
                return ConfigFileResult<ConfigFileEntryModel>.Fail(new ConfigFileError(ConfigFileErrorCode.EntryNotFound, $"Entry not found: {key}"));
            }

            public ConfigFileResult<string> EncodeDescription()
            {
                if (string.IsNullOrEmpty(Description))
                    return string.Empty;
                var description = Description.Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                var sb = new StringBuilder();
                foreach (var line in description)
                    sb.AppendLine($"## {line}");
                return sb.ToString().Trim();
            }

            public ConfigFileResult<string> EncodeTableHeader()
            {
                if (!IsValidTableName(TableName))
                    return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Invalid table name: {TableName}"));
                return $"[{TableName}]";
            }

            public ConfigFileResult<string> EncodeTable()
            {
                var descriptionResult = EncodeDescription();
                if (!descriptionResult.Success)
                    return ConfigFileResult<string>.Fail(descriptionResult.Errors);

                var tableHeaderResult = EncodeTableHeader();
                if (!tableHeaderResult.Success)
                    return ConfigFileResult<string>.Fail(tableHeaderResult.Errors);

                var sb = new StringBuilder();
                var result = new ConfigFileResult<string>();

                if (descriptionResult.Value != string.Empty)
                    sb.AppendLine(descriptionResult.Value);

                sb.AppendLine(tableHeaderResult.Value);
                sb.AppendLine();

                foreach (var entry in Entries.Values)
                {
                    var entryResult = ((ConfigFileEntryModel)entry).EncodeEntry();
                    if (entryResult.Success)
                        sb.AppendLine(entryResult.Value);
                    else
                        result.AddError(entryResult.Errors);
                    sb.AppendLine();
                }

                result.SetValue(sb.ToString().Trim());
                return result;
            }

            public static bool IsValidTableName(string tableName)
            {
                if (string.IsNullOrWhiteSpace(tableName))
                    return false;
                if (tableName.All(c => char.IsLetterOrDigit(c) || c == '_'))
                    return true;
                return false;
            }

            public static ConfigFileResult<Table> Create(string tableName, string description)
            {
                if (!IsValidTableName(tableName))
                    return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Invalid table name: {tableName}"));
                var table = new Table(tableName, description);
                return table;
            }

            public static ConfigFileResult<Table> DecodeTable(string[] content, ref int index)
            {
                var headerResult = DecodeTableHeader(content, ref index);
                if (!headerResult.Success)
                    return ConfigFileResult<Table>.Fail(headerResult.Errors);

                var table = new Table(headerResult.Value.TableName, headerResult.Value.Description);
                var result = new ConfigFileResult<Table>(table, true, null);

                for (var i = index; index < content.Length && !DecodeTableHeader(content, ref i); i = index)
                {
                    var entryResult = ConfigFileEntryModel.DecodeEntry(content, ref index);
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

            public static ConfigFileResult<Table> DecodeTableHeader(string[] content, ref int index)
            {
                foreach (var item in content.Skip(index))
                {
                    index++;
                    var line = item.Trim();

                    if (string.IsNullOrWhiteSpace(line) || line.StartsWith("#"))
                        continue;

                    if (line.StartsWith("[") && line.EndsWith("]"))
                    {
                        var tableName = line.Substring(1, line.Length - 2);
                        var tableResult = Create(tableName, string.Empty);
                        if (!tableResult.Success)
                            return ConfigFileResult<Table>.Fail(tableResult.Errors);
                        return tableResult;
                    }

                    return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableHeader, $"Invalid table header: {line}"));
                }

                return ConfigFileResult<Table>.Fail(new ConfigFileError(ConfigFileErrorCode.EndOfContent, "No more content to process"));
            }
        }
    }
}
