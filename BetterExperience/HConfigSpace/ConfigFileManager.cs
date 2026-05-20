using BetterExperience.HTranslatorSpace;
using System;
using System.IO;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 管理单个配置文件与运行时配置表之间的绑定关系。
    /// 该类型负责读取/写入文件、创建表项、把磁盘上的 <see cref="ConfigFileEntry"/> 重新绑定到运行时 <see cref="ConfigEntry{T}"/>。
    /// 它不负责 UI 展示和具体配置项声明；这些职责分别由配置 GUI 与 <c>ConfigManager</c> 承担。
    /// </summary>
    public class ConfigFileManager
    {
        /// <summary>
        /// 控制配置项赋值后是否立即写回文件。
        /// 批量初始化或重载期间应临时关闭，避免每个配置项都触发一次磁盘写入。
        /// </summary>
        public bool SaveOnConfigSet { get; set; } = true;

        /// <summary>
        /// 从磁盘解析得到的文件模型，保存原始配置表和配置项。
        /// </summary>
        public ConfigFileSheet FileSheet { get; private set; }

        /// <summary>
        /// 运行时绑定后的配置模型，供业务代码直接访问强类型配置项。
        /// </summary>
        public ConfigSheet Sheet { get; private set; }

        /// <summary>
        /// 配置文件路径。修改该路径不会自动迁移旧文件，下一次读写会使用新路径。
        /// </summary>
        public string FilePath { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        /// <summary>
        /// 创建配置文件管理器并立即尝试读取指定路径的配置。
        /// </summary>
        /// <param name="filePath">配置文件路径；文件不存在时会创建空的内存模型，实际文件在写入时生成。</param>
        public ConfigFileManager(string filePath)
        {
            FilePath = filePath;
            Sheet = new ConfigSheet();
            Read();
        }

        /// <summary>
        /// 从 <see cref="FilePath"/> 读取并解析配置文件。
        /// 读取失败时保留错误日志并返回 <c>false</c>，不会向调用方抛出文件系统异常。
        /// </summary>
        /// <returns>读取流程是否完成。解析错误会记录日志，但当前实现仍返回 <c>true</c> 并保存可解析出的模型。</returns>
        public bool Read()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    FileSheet = new ConfigFileSheet();
                    return true;
                }

                var content = File.ReadAllText(FilePath).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                int index = 0;
                var decodeResult = ConfigFileSheet.DecodeSheet(content, ref index);
                if (!decodeResult.Success)
                {
                    foreach (var error in decodeResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, index);
                }
                FileSheet = decodeResult.Value;
                return true;
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to read config file: {FilePath}.", ex);
                return false;
            }
        }

        /// <summary>
        /// 将当前文件模型编码并写入 <see cref="FilePath"/>。
        /// 写入先落到同目录临时文件，再替换目标文件，以降低写入中断留下半截配置的风险。
        /// </summary>
        /// <returns>编码与写入是否成功；失败时记录日志并返回 <c>false</c>。</returns>
        public bool Write()
        {
            var directoryPath = string.Empty;
            var tmpFilePath = string.Empty;

            try
            {
                var encodeResult = FileSheet.EncodeSheet();
                if (!encodeResult.Success)
                {
                    foreach (var error in encodeResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    return false;
                }

                directoryPath = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                tmpFilePath = Path.Combine(directoryPath ?? string.Empty, $"{Path.GetFileName(FilePath)}.{Guid.NewGuid():N}.tmp");
                var backupFilePath = FilePath + ".bak";

                File.WriteAllText(tmpFilePath, encodeResult.Value);

                if (File.Exists(FilePath))
                    File.Replace(tmpFilePath, FilePath, backupFilePath, true);
                else
                    File.Move(tmpFilePath, FilePath);

                return true;
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to write config file: {FilePath}.", ex);
                return false;
            }
            finally
            {
                try
                {
                    if (File.Exists(tmpFilePath))
                        File.Delete(tmpFilePath);
                }
                catch
                {

                }
            }
        }

        /// <summary>
        /// 重新读取磁盘配置，并把已声明配置项重新绑定到新的文件项。
        /// 元数据会从当前运行时配置同步回文件项，但不会覆盖用户在文件中的配置值。
        /// </summary>
        public void Reload()
        {
            var oldSaveOnConfigSet = SaveOnConfigSet;
            SaveOnConfigSet = false;

            Read();
            foreach (var table in Sheet.Values)
            {
                foreach (var entry in table)
                {
                    var entryResult = FileSheet.GetEntry(entry.TableName, entry.Key);
                    if (entryResult.Success)
                    {
                        entry.Entry.CopyTo(entryResult.Value, false);
                        entry.RebindEntry(entryResult.Value);
                    }
                }
            }

            SaveOnConfigSet = oldSaveOnConfigSet;
            Save();
        }

        /// <summary>
        /// 绑定一个强类型配置项；如果文件中不存在该项，则使用默认值创建。
        /// </summary>
        /// <typeparam name="T">配置值类型，必须能被 <see cref="ConfigFileEntry"/> 编码和解码。</typeparam>
        /// <param name="tableKey">已有配置表键名。</param>
        /// <param name="key">配置项键名，只允许字母、数字和下划线。</param>
        /// <param name="defaultValue">文件中缺失该项时写入的默认值。</param>
        /// <param name="entryName">用于生成配置文件注释和 GUI 标签的名称。</param>
        /// <param name="description">用于生成配置文件注释和 GUI 提示的描述。</param>
        /// <returns>可在运行时读写的强类型配置项。</returns>
        /// <exception cref="ArgumentException">表不存在或键名非法时抛出。</exception>
        /// <exception cref="InvalidOperationException">默认值无法编码或文件模型无法接受新项时抛出。</exception>
        public ConfigEntry<T> Bind<T>(string tableKey, string key, T defaultValue, Translator entryName, Translator description)
        {
            ConfigEntry<T> result = null;
            var entryResult = FileSheet.GetEntry(tableKey, key);
            if (entryResult.Success)
            {
                result = new ConfigEntry<T>(tableKey, entryResult.Value, defaultValue, entryName, description);
            }
            else
            {
                var tableResult = FileSheet.GetTable(tableKey);
                if (!tableResult.Success)
                {
                    foreach (var error in tableResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new ArgumentException($"Config table not found: {tableKey}.", nameof(tableKey));
                }
                var newEntry = new ConfigFileEntry();

                if (!ConfigFileEntry.IsValidKeyName(key))
                    throw new ArgumentException($"Invalid key name for config entry: {tableKey}.{key}.", nameof(key));
                newEntry.Key = key;

                var valueResult = ConfigFileEntry.EncodeValue(defaultValue);
                if (!valueResult.Success)
                {
                    foreach (var error in valueResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new InvalidOperationException($"Failed to encode default value for config entry: {tableKey}.{key}. Errors: {string.Join(", ", valueResult.Errors)}");
                }
                newEntry.Value = valueResult.Value;

                var addEntryResult = tableResult.Value.AddEntry(newEntry);
                if (!addEntryResult.Success)
                {
                    foreach (var error in addEntryResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new InvalidOperationException($"Failed to add config entry to table: {tableKey}.{key}.");
                }

                result = new ConfigEntry<T>(tableKey, newEntry, defaultValue, entryName, description);
            }

            result.OnValueChanged += OnConfigEntryChanged;

            Sheet[tableKey].Add(result);
            return result;
        }

        /// <summary>
        /// 声明一个配置表；文件中不存在时会创建对应表结构。
        /// </summary>
        /// <param name="tableKey">表键名，只允许字母、数字和下划线。</param>
        /// <param name="tableName">运行时展示名称。</param>
        /// <param name="description">写入配置文件的表说明，可为空。</param>
        public void CreateTable(string tableKey, Translator tableName, Translator description = null)
        {
            var tableResult = FileSheet.GetTable(tableKey);
            if (tableResult.Success)
            {
                if (!Sheet.Contains(tableKey))
                    Sheet.Add(tableKey, new ConfigTable(tableKey, tableResult.Value, tableName, description));
                return;
            }

            var newTableResult = ConfigFileSheet.CreateTable(tableKey, description);
            if (!newTableResult.Success)
            {
                foreach (var error in newTableResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to create config table: {tableKey}.");
            }

            var addTableResult = FileSheet.AddTable(newTableResult.Value);
            if (!addTableResult.Success)
            {
                foreach (var error in addTableResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to add config table: {tableKey}.");
            }

            Sheet.Add(tableKey, new ConfigTable(tableKey, newTableResult.Value, tableName, description));
        }

        /// <summary>
        /// 保存当前配置文件模型。
        /// </summary>
        public void Save()
        {
            Write();
        }

        private void OnConfigEntryChanged<T>(object sender, T newValue)
        {
            if (SaveOnConfigSet)
                Write();
        }
    }
}
