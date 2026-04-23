using BetterExperience.HTranslatorSpace;
using System;
using System.IO;

namespace BetterExperience.HConfigSpace
{
    public class ConfigFileManager
    {
        public bool SaveOnConfigSet { get; set; } = true;

        public ConfigFileSheet FileSheet { get; private set; }

        public ConfigSheet Sheet { get; private set; }

        public string FilePath { get; set; }

        public string FileName => Path.GetFileName(FilePath);

        public ConfigFileManager(string filePath)
        {
            FilePath = filePath;
            Sheet = new ConfigSheet();
            Read();
        }

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

        public bool Write()
        {
            var tmpFilePath = FilePath + ".tmp";
            try
            {
                var encodeResult = FileSheet.EncodeSheet();
                if (!encodeResult.Success)
                {
                    foreach (var error in encodeResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    return false;
                }

                var directoryPath = Path.GetDirectoryName(FilePath);
                if (!string.IsNullOrWhiteSpace(directoryPath) && !Directory.Exists(directoryPath))
                    Directory.CreateDirectory(directoryPath);

                File.WriteAllText(tmpFilePath, encodeResult.Value);

                if (File.Exists(FilePath))
                    File.Delete(FilePath);

                File.Move(tmpFilePath, FilePath);

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
