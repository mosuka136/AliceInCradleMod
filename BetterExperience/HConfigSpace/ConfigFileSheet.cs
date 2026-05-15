using BetterExperience.HTranslatorSpace;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 表示整个配置文件的内存模型。
    /// 该类型按文件顺序保存多个配置表，负责整文件级别的编码/解码协调，不负责磁盘 IO。
    /// </summary>
    public class ConfigFileSheet
    {
        /// <summary>
        /// 配置表集合，使用有序字典保持保存后的表顺序稳定。
        /// </summary>
        public OrderedDictionary Sheet { get; private set; } = new OrderedDictionary();

        public ConfigFileResult<ConfigFileTable> AddTable(ConfigFileTable table)
        {
            return AddTable(table.Key, table);
        }

        public ConfigFileResult<ConfigFileTable> AddTable(string tableKey, ConfigFileTable table)
        {
            if (!ConfigFileTable.IsValidTableName(tableKey))
                return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Invalid table name: {tableKey}"));
            if (table == null)
                return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, "Table cannot be null"));
            if (Sheet.Contains(tableKey))
                return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Table name already exists: {tableKey}"));
            Sheet.Add(tableKey, table);
            return table;
        }

        public ConfigFileResult<ConfigFileTable> GetTable(string tableKey)
        {
            if (Sheet.Contains(tableKey))
                return (ConfigFileTable)Sheet[tableKey];
            return ConfigFileResult<ConfigFileTable>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, $"Table not found: {tableKey}"));
        }

        public ConfigFileResult<ConfigFileEntry> GetEntry(string tableKey, string key)
        {
            if (Sheet.Contains(tableKey))
            {
                var table = (ConfigFileTable)Sheet[tableKey];
                return table.GetEntry(key);
            }
            return ConfigFileResult<ConfigFileEntry>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, $"Table not found: {tableKey}"));
        }

        public static ConfigFileResult<ConfigFileTable> CreateTable(string tableKey, Translator description)
        {
            var table = ConfigFileTable.Create(tableKey, description);
            if (!table.Success)
                return ConfigFileResult<ConfigFileTable>.Fail(table.Errors);
            return table.Value;
        }

        public ConfigFileResult<string> EncodeSheet()
        {
            var sb = new StringBuilder();
            var result = new ConfigFileResult<string>();
            foreach (ConfigFileTable table in Sheet.Values)
            {
                var tableResult = table.EncodeTable();
                if (tableResult.Success)
                    sb.AppendLine(tableResult.Value);
                else
                    result.AddError(tableResult.Errors);
                sb.AppendLine();
            }

            if (result.Errors.Count > 0)
                return ConfigFileResult<string>.Fail(result.Errors);

            result.SetValue(sb.ToString().Trim());
            return result;
        }

        /// <summary>
        /// 从按行拆分后的配置内容解析整个配置模型。
        /// </summary>
        /// <param name="content">配置文件内容行。</param>
        /// <param name="index">读取起点；返回时推进到已消费位置。</param>
        /// <returns>配置文件模型。单个表解析失败时会收集错误并继续尝试后续内容。</returns>
        public static ConfigFileResult<ConfigFileSheet> DecodeSheet(string[] content, ref int index)
        {
            var model = new ConfigFileSheet();
            var result = new ConfigFileResult<ConfigFileSheet>(model, true, null);

            while (index < content.Length)
            {
                var tableResult = ConfigFileTable.DecodeTable(content, ref index);
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
    }
}
