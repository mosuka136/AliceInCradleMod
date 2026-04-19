using BetterExperience.HTranslatorSpace;
using System.Collections.Specialized;
using System.Linq;
using System.Text;

namespace BetterExperience.HConfigFileSpace
{
    public class ConfigFileSheetModel
    {
        public OrderedDictionary Sheet { get; private set; } = new OrderedDictionary();

        public ConfigFileResult<ConfigFileTableModel> AddTable(ConfigFileTableModel table)
        {
            return AddTable(table.Key, table);
        }

        public ConfigFileResult<ConfigFileTableModel> AddTable(string tableKey, ConfigFileTableModel table)
        {
            if (!ConfigFileTableModel.IsValidTableName(tableKey))
                return ConfigFileResult<ConfigFileTableModel>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Invalid table name: {tableKey}"));
            if (table == null)
                return ConfigFileResult<ConfigFileTableModel>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, "Table cannot be null"));
            if (Sheet.Contains(tableKey))
                return ConfigFileResult<ConfigFileTableModel>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidTableName, $"Table name already exists: {tableKey}"));
            Sheet.Add(tableKey, table);
            return table;
        }

        public ConfigFileResult<ConfigFileTableModel> GetTable(string tableKey)
        {
            if (Sheet.Contains(tableKey))
                return (ConfigFileTableModel)Sheet[tableKey];
            return ConfigFileResult<ConfigFileTableModel>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, $"Table not found: {tableKey}"));
        }

        public ConfigFileResult<ConfigFileEntryModel> GetEntry(string tableKey, string key)
        {
            if (Sheet.Contains(tableKey))
            {
                var table = (ConfigFileTableModel)Sheet[tableKey];
                return table.GetEntry(key);
            }
            return ConfigFileResult<ConfigFileEntryModel>.Fail(new ConfigFileError(ConfigFileErrorCode.TableNotFound, $"Table not found: {tableKey}"));
        }

        public static ConfigFileResult<ConfigFileTableModel> CreateTable(string tableKey, Translator description)
        {
            var table = ConfigFileTableModel.Create(tableKey, description);
            if (!table.Success)
                return ConfigFileResult<ConfigFileTableModel>.Fail(table.Errors);
            return table.Value;
        }

        public ConfigFileResult<string> EncodeSheet()
        {
            var sb = new StringBuilder();
            var result = new ConfigFileResult<string>();
            foreach (ConfigFileTableModel table in Sheet.Values)
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

        public static ConfigFileResult<ConfigFileSheetModel> DecodeSheet(string[] content, ref int index)
        {
            var model = new ConfigFileSheetModel();
            var result = new ConfigFileResult<ConfigFileSheetModel>(model, true, null);

            while (index < content.Length)
            {
                var tableResult = ConfigFileTableModel.DecodeTable(content, ref index);
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
