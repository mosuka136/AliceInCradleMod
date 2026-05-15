using BetterExperience.HTranslatorSpace;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 表示配置文件中的一个表段。
    /// 表段按插入顺序保存配置项，负责编码/解码 <c>[Table]</c> 头和表内键值项，不处理跨表级别的文件结构。
    /// </summary>
    public class ConfigFileTable
    {
        /// <summary>
        /// 写入文件的表名称注释；当前解析流程不会从已有注释中恢复该值。
        /// </summary>
        public Translator Name { get; set; }
        /// <summary>
        /// 写入文件的表说明注释；当前解析流程不会从已有注释中恢复该值。
        /// </summary>
        public Translator Description { get; set; }
        /// <summary>
        /// 表键名，对应配置文件中的 <c>[Key]</c>。
        /// </summary>
        public string Key { get; set; }
        /// <summary>
        /// 表内配置项，使用有序字典以保持写出顺序稳定。
        /// </summary>
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

        /// <summary>
        /// 判断表名是否符合配置文件语法约束。
        /// </summary>
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

        /// <summary>
        /// 从当前位置解析一个完整表段。
        /// </summary>
        /// <param name="content">按行拆分后的配置文件内容。</param>
        /// <param name="index">读取起点；返回时推进到下一个表头或文件结尾。</param>
        /// <returns>解析出的表段。表内部分配置项失败时会保留已成功解析的项并收集错误。</returns>
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

        /// <summary>
        /// 从当前位置查找并解析下一个表头。
        /// </summary>
        /// <param name="content">按行拆分后的配置文件内容。</param>
        /// <param name="index">读取起点；成功时推进到表头后一行。</param>
        /// <returns>只包含表键名的表模型；没有更多内容时返回 <see cref="ConfigFileErrorCode.EndOfContent"/>。</returns>
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
