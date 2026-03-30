using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;

namespace BetterExperience.ConfigFileSpace
{
    public class ConfigFileManager
    {
        public bool SaveOnConfigSet { get; set; } = true;

        public ConfigFileTablesModel FileTables { get; private set; }

        public OrderedDictionary Tables { get; private set; }

        public string FilePath { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public ConfigFileManager(string filePath)
        {
            FilePath = filePath;
            Tables = new OrderedDictionary();
            Read();
        }

        public bool Read()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    FileTables = new ConfigFileTablesModel();
                    return true;
                }

                var content = File.ReadAllText(FilePath).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                int index = 0;
                var decodeResult = ConfigFileTablesModel.DecodeTables(content, ref index);
                if (!decodeResult.Success)
                {
                    foreach (var error in decodeResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, index);
                }
                FileTables = decodeResult.Value;
                return true;
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to read config file: {FilePath}.", ex);
                return false;
            }
        }

        public bool Write()
        {
            var oldFilePath = Path.Combine(Path.GetDirectoryName(FilePath), $"{Path.GetFileNameWithoutExtension(FilePath)}_old{Path.GetExtension(FilePath)}");
            try
            {
                var encodeResult = FileTables.EncodeTables();
                if (!encodeResult.Success)
                {
                    foreach (var error in encodeResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    return false;
                }

                var directoryPath = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                if (!File.Exists(FilePath))
                    File.WriteAllText(FilePath, encodeResult.Value);
                else
                {
                    File.Move(FilePath, oldFilePath);
                    File.WriteAllText(FilePath, encodeResult.Value);
                    File.Delete(oldFilePath);
                }

                return true;
            }
            catch (Exception ex)
            {
                HLog.Error($"Failed to write config file: {FilePath}.", ex);
                return false;
            }
        }

        public void Reload()
        {
            var oldSaveOnConfigSet = SaveOnConfigSet;
            SaveOnConfigSet = false;

            Read();
            foreach (ConfigTable table in Tables.Values)
            {
                foreach (var entry in table.Table)
                {
                    var entryResult = FileTables.GetEntry(entry.TableName, entry.Key);
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

        public ConfigEntry<T> Bind<T>(string tableName, string key, T defaultValue, string description)
        {
            HLog.Info($"Rebinding config entry");

            ConfigEntry<T> result = null;
            var entryResult = FileTables.GetEntry(tableName, key);
            if (entryResult.Success)
            {
                result = new ConfigEntry<T>(tableName, entryResult.Value, defaultValue, description);
            }
            else
            {
                var tableResult = FileTables.GetTable(tableName);
                if (!tableResult.Success)
                {
                    foreach (var error in tableResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new Exception($"Config table not found: {tableName}.");
                }
                var newEntry = new ConfigFileEntryModel();

                if (!ConfigFileEntryModel.IsValidKeyName(key))
                    throw new Exception($"Invalid key name for config entry: {tableName}.{key}.");
                newEntry.Key = key;

                var valueResult = ConfigFileEntryModel.EncodeValue(defaultValue);
                if (!valueResult.Success)
                {
                    foreach (var error in valueResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new Exception($"Failed to encode default value for config entry: {tableName}.{key}. Errors: {string.Join(", ", valueResult.Errors)}");
                }
                newEntry.Value = valueResult.Value;

                var addEntryResult = tableResult.Value.AddEntry(newEntry);
                if (!addEntryResult.Success)
                {
                    foreach (var error in addEntryResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new Exception($"Failed to add config entry to table: {tableName}.{key}.");
                }

                result = new ConfigEntry<T>(tableName, newEntry, defaultValue, description);
            }

            result.SettingChanged += OnConfigEntryChanged;

            if (!Tables.Contains(tableName))
                throw new Exception($"Config table '{tableName}' is missing from the manager.");
            if (!(Tables[tableName] is ConfigTable table))
                throw new Exception($"Config table is not a list: {tableName}.");

            table.Table.Add(result);
            return result;
        }

        public void CreateTable(string tableName, string description = "")
        {
            var tableResult = FileTables.GetTable(tableName);
            if (tableResult.Success)
            {
                if (!Tables.Contains(tableName))
                    Tables.Add(tableName, new ConfigTable(tableName, description));
                return;
            }

            var newTableResult = ConfigFileTablesModel.CreateTable(tableName, description);
            if (!newTableResult.Success)
            {
                foreach (var error in newTableResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new Exception($"Failed to create config table: {tableName}.");
            }

            var addTableResult = FileTables.AddTable(newTableResult.Value);
            if (!addTableResult.Success)
            {
                foreach (var error in addTableResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new Exception($"Failed to create config table: {tableName}.");
            }

            if (Tables.Contains(tableName))
                throw new Exception($"Config table already exists in manager: {tableName}.");

            Tables.Add(tableName, new ConfigTable(tableName, description));
        }

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
