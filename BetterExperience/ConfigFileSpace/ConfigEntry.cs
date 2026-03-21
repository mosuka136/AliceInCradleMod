using System;
using System.Collections;
using System.Collections.Generic;

namespace BetterExperience.ConfigFileSpace
{
    public interface IConfigEntry
    {
        string Description { get; }
        string TableName { get; }
        string Key { get; }
        ConfigFileEntryModel Entry { get; }

        void RebindEntry(ConfigFileEntryModel entry);
    }

    public class ConfigEntry<T> : IConfigEntry
    {
        private T _value;

        public T Value
        {
            get => _value;
            set
            {
                if (Equal(value, _value))
                    return;

                var valueResult = ConfigFileEntryModel.EncodeValue(value);
                if (!valueResult.Success)
                {
                    foreach (var error in valueResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new InvalidOperationException($"Failed to encode value for key: {Key}, value: {value}. Errors: {string.Join(", ", valueResult.Errors)}");
                }

                _value = value;

                Entry.Value = valueResult.Value;
                SettingChanged?.Invoke(this, _value);
            }
        }

        public string Description => Entry.Description;
        public string TableName { get; private set; }
        public string Key => Entry.Key;
        public T DefaultValue { get; private set; }
        public ConfigFileEntryModel Entry { get; private set; }

        public event EventHandler<T> SettingChanged;

        public ConfigEntry()
        {

        }

        public ConfigEntry(string tableName, ConfigFileEntryModel entry, T defaultValue) :
            this(tableName, entry, defaultValue, entry.Description)
        {
        }

        public ConfigEntry(string tableName, ConfigFileEntryModel entry, T defaultValue, string description)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            entry.Description = description;

            var valueTypeResult = ConfigFileEntryModel.EncodeValueType<T>();
            if (!valueTypeResult.Success)
            {
                foreach (var error in valueTypeResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to encode value type for key: {Key}, type: {typeof(T).FullName}. Errors: {string.Join(", ", valueTypeResult.Errors)}");
            }
            entry.ValueType = valueTypeResult.Value;

            var defaultValueResult = ConfigFileEntryModel.EncodeValue(defaultValue);
            if (!defaultValueResult.Success)
            {
                foreach (var error in defaultValueResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to encode default value for key: {Key}, value: {defaultValue}. Errors: {string.Join(", ", defaultValueResult.Errors)}");
            }
            entry.DefaultValue = defaultValueResult.Value;

            if (!ConfigFileEntryModel.IsValidKeyName(entry.Key))
                throw new InvalidOperationException($"Invalid key name: {entry.Key}");

            if (!ConfigFileTableModel.Table.IsValidTableName(tableName))
                throw new InvalidOperationException($"Invalid table name: {tableName}");

            TableName = tableName;
            DefaultValue = defaultValue;
            RebindEntry(entry);
        }

        public void RebindEntry(ConfigFileEntryModel entry)
        {
            if (entry == null)
                return;
            var decodeResult = ConfigFileEntryModel.DecodeValue<T>(entry.Value);
            if (!decodeResult.Success)
            {
                foreach (var error in decodeResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to decode value for key: {Key}, value: {entry.Value}. Errors: {string.Join(", ", decodeResult.Errors)}");
            }
            Entry = entry;
            Value = decodeResult.Value;
        }

        public static bool Equal(T a, T b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            var type = a.GetType();

            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return EqualityComparer<T>.Default.Equals(a, b);

            if (type.IsArray)
            {
                var arrayA = a as Array;
                var arrayB = b as Array;

                if (arrayA == null || arrayB == null)
                    return false;

                if (arrayA.Length != arrayB.Length)
                    return false;

                for (int i = 0; i < arrayA.Length; i++)
                {
                    var elementA = arrayA.GetValue(i);
                    var elementB = arrayB.GetValue(i);
                    if (!ConfigEntry<object>.Equal(elementA, elementB))
                        return false;
                }
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var enumA = (a as IEnumerable)?.GetEnumerator();
                var enumB = (b as IEnumerable)?.GetEnumerator();

                if (enumA == null || enumB == null)
                    return false;

                while (true)
                {
                    var hasNextA = enumA.MoveNext();
                    var hasNextB = enumB.MoveNext();

                    if (hasNextA != hasNextB)
                        return false;

                    if (!hasNextA)
                        break;

                    if (!ConfigEntry<object>.Equal(enumA.Current, enumB.Current))
                        return false;
                }

                return true;
            }

            return false;
        }
    }
}
