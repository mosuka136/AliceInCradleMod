using System;
using System.Collections.Generic;
using System.IO;

namespace BetterExperience.ConfigFileSpace
{
    public class ConfigFileManager
    {
        public bool SaveOnConfigSet { get; set; } = true;

        public ConfigFileTableModel Tables { get; private set; }

        public List<IConfigEntry> ConfigEntries { get; private set; }

        public string FilePath { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public ConfigFileManager(string filePath)
        {
            FilePath = filePath;
            ConfigEntries = new List<IConfigEntry>();
            Read();
        }

        public bool Read()
        {
            try
            {
                if (!File.Exists(FilePath))
                {
                    Tables = new ConfigFileTableModel();
                    return true;
                }

                var content = File.ReadAllText(FilePath).Replace("\r\n", "\n").Replace("\r", "\n").Split('\n');
                int index = 0;
                var decodeResult = ConfigFileTableModel.DecodeTables(content, ref index);
                Tables = decodeResult.Value;
                if (!decodeResult.Success)
                {
                    foreach (var error in decodeResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, index);
                }
                Tables = decodeResult.Value;
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
                var encodeResult = Tables.EncodeTables();
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
            Read();
            foreach (var entry in ConfigEntries)
            {
                var entryResult = Tables.GetEntry(entry.TableName, entry.Key);
                if (entryResult.Success)
                    entry.RebindEntry(entryResult.Value);
            }
        }

        public ConfigEntry<T> Bind<T>(string tableName, string key, T defaultValue, string description)
        {
            ConfigEntry<T> result = null;
            var entryResult = Tables.GetEntry(tableName, key);
            if (entryResult.Success)
            {
                result = new ConfigEntry<T>(tableName, entryResult.Value, defaultValue, description);
            }
            else
            {
                var tableResult = Tables.GetTable(tableName);
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
            ConfigEntries.Add(result);
            return result;
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
