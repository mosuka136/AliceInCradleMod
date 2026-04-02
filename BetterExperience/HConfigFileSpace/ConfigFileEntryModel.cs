using BetterExperience.HTranslatorSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BetterExperience.HConfigFileSpace.ConfigFileModel;

namespace BetterExperience.HConfigFileSpace
{
    public class ConfigFileEntryModel
    {
        private string _key;

        public Translator Name { get; set; }
        public Translator Description { get; set; }
        public string Key
        {
            get => _key;

            set
            {
                if (IsValidKeyName(value))
                    _key = value;
                else
                    throw new ArgumentException($"Invalid key name: {value}. Key names must be non-empty and can only contain letters, digits, and underscores.");
            }
        }
        public string Value { get; set; }
        public string DefaultValue { get; set; }
        public string ValueType { get; set; }
        public string AcceptableValues { get; set; }

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
            return sb.ToString().TrimEnd();
        }

        public ConfigFileResult<string> EncodeValueType()
        {
            if (string.IsNullOrEmpty(ValueType))
                return string.Empty;
            return $"# Value Type: {ValueType}";
        }

        public ConfigFileResult<string> EncodeAcceptableValues()
        {
            if (string.IsNullOrEmpty(AcceptableValues))
                return string.Empty;
            return $"# Acceptable Values: {AcceptableValues}";
        }

        public ConfigFileResult<string> EncodeDefaultValue()
        {
            if (string.IsNullOrEmpty(DefaultValue))
                return string.Empty;
            return $"# Default Value: {DefaultValue}";
        }

        public ConfigFileResult<string> EncodeKeyValuePair()
        {
            if (!IsValidKeyName(_key))
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyName, $"Invalid key name: {Key}"));
            if (string.IsNullOrWhiteSpace(Value))
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Value cannot be empty for key: {Key}"));
            return $"{Key} = {Value}";
        }

        public ConfigFileResult<string> EncodeEntry()
        {
            var nameResult = EncodeName();
            if (!nameResult.Success)
                return ConfigFileResult<string>.Fail(nameResult.Errors);

            var descriptionResult = EncodeDescription();
            if (!descriptionResult.Success)
                return ConfigFileResult<string>.Fail(descriptionResult.Errors);

            var valueTypeResult = EncodeValueType();
            if (!valueTypeResult.Success)
                return ConfigFileResult<string>.Fail(valueTypeResult.Errors);

            var acceptableValuesResult = EncodeAcceptableValues();
            if (!acceptableValuesResult.Success)
                return ConfigFileResult<string>.Fail(acceptableValuesResult.Errors);

            var defaultValueResult = EncodeDefaultValue();
            if (!defaultValueResult.Success)
                return ConfigFileResult<string>.Fail(defaultValueResult.Errors);

            var keyValuePairResult = EncodeKeyValuePair();
            if (!keyValuePairResult.Success)
                return ConfigFileResult<string>.Fail(keyValuePairResult.Errors);

            var sb = new StringBuilder();
            if (nameResult.Value != string.Empty)
                sb.AppendLine(nameResult.Value);
            if (descriptionResult.Value != string.Empty)
                sb.AppendLine(descriptionResult.Value);
            if (valueTypeResult.Value != string.Empty)
                sb.AppendLine(valueTypeResult.Value);
            if (acceptableValuesResult.Value != string.Empty)
                sb.AppendLine(acceptableValuesResult.Value);
            if (defaultValueResult.Value != string.Empty)
                sb.AppendLine(defaultValueResult.Value);
            sb.AppendLine(keyValuePairResult.Value);

            return sb.ToString().Trim();
        }

        public bool CopyTo(ConfigFileEntryModel target, bool overrideValue)
        {
            if (target == null)
                return false;

            target.Name = Name;
            target.Description = Description;
            target.Key = Key;
            target.DefaultValue = DefaultValue;
            target.ValueType = ValueType;
            target.AcceptableValues = AcceptableValues;
            if (overrideValue)
                target.Value = Value;
            return true;
        }

        public static bool IsValidKeyName(string key)
        {
            return !string.IsNullOrWhiteSpace(key) && key.All(c => char.IsLetterOrDigit(c) || c == '_');
        }

        public static bool IsKeyValuePair(string content)
        {
            content = content.Trim();
            return content.Contains('=') && !content.StartsWith("#");
        }

        public static bool IsComment(string content)
        {
            return content.TrimStart().StartsWith("#");
        }

        public static ConfigFileResult<string> EncodeValue<T>(T value)
        {
            return Encode(value);
        }

        public static ConfigFileResult<string> EncodeValueType<T>()
        {
            return EncodeValueType(typeof(T));
        }

        public static ConfigFileResult<string> EncodeValueType(Type type)
        {
            switch (type)
            {
                case Type t when t == typeof(string):
                    return "String";
                case Type t when t == typeof(sbyte):
                    return "Int8";
                case Type t when t == typeof(short):
                    return "Int16";
                case Type t when t == typeof(int):
                    return "Int32";
                case Type t when t == typeof(long):
                    return "Int64";
                case Type t when t == typeof(byte):
                    return "UInt8";
                case Type t when t == typeof(ushort):
                    return "UInt16";
                case Type t when t == typeof(uint):
                    return "UInt32";
                case Type t when t == typeof(ulong):
                    return "UInt64";
                case Type t when t == typeof(float):
                    return "Float";
                case Type t when t == typeof(double):
                    return "Double";
                case Type t when t == typeof(bool):
                    return "Boolean";
                default:
                    break;
            }

            if (type.IsEnum)
                return $"Enum {type.Name}";

            if (typeof(IEnumerable).IsAssignableFrom(type))
                return $"{EncodeValueType(GetCollectionElementType(type))}[]";

            return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, $"Unsupported type: {type.FullName}"));
        }

        public static ConfigFileResult<string> EncodeAcceptableValues<T>()
        {
            return EncodeAcceptableValues(typeof(T));
        }

        public static ConfigFileResult<string> EncodeAcceptableValues(Type type)
        {
            if (typeof(IEnumerable).IsAssignableFrom(type))
                return EncodeAcceptableValues(GetCollectionElementType(type));

            if (!type.IsEnum)
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidType, $"Type is not an enum: {type.FullName}"));

            var enumNames = Enum.GetNames(type);
            var acceptableValues = string.Join(", ", enumNames);
            return acceptableValues;
        }

        public static ConfigFileResult<ConfigFileEntryModel> CreateEntry<T>(string key, T value, T defaultValue, Translator name, Translator description)
        {
            if (!IsValidKeyName(key))
                return ConfigFileResult<ConfigFileEntryModel>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyName, $"Invalid key name: {key}"));

            var encodedValueResult = EncodeValue(value);
            if (!encodedValueResult.Success)
                return ConfigFileResult<ConfigFileEntryModel>.Fail(encodedValueResult.Errors);

            var encodedDefaultValueResult = EncodeValue(defaultValue);
            if (!encodedDefaultValueResult.Success)
                return ConfigFileResult<ConfigFileEntryModel>.Fail(encodedDefaultValueResult.Errors);

            var encodedValueTypeResult = EncodeValueType<T>();
            if (!encodedValueTypeResult.Success)
                return ConfigFileResult<ConfigFileEntryModel>.Fail(encodedValueTypeResult.Errors);

            var entry = new ConfigFileEntryModel
            {
                Name = name,
                Description = description,
                Key = key,
                Value = encodedValueResult.Value,
                DefaultValue = encodedDefaultValueResult.Value,
                ValueType = encodedValueTypeResult.Value
            };

            return entry;
        }

        public static ConfigFileResult<T> DecodeValue<T>(string value)
        {
            return Decode<T>(value);
        }

        public static ConfigFileResult<(string, string)> DecodeKeyValuePair(string content)
        {
            if (!IsKeyValuePair(content))
                return ConfigFileResult<(string, string)>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyValuePair, $"Invalid key-value pair: {content}"));

            var parts = content.Split(new[] { '=' }, 2);
            var key = parts[0].Trim();
            var value = parts[1].Trim();

            if (!IsValidKeyName(key))
                return ConfigFileResult<(string, string)>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyName, $"Invalid key name: {key}"));

            if (string.IsNullOrWhiteSpace(value))
                return ConfigFileResult<(string, string)>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Value cannot be empty for key: {key}"));

            return (key, value);
        }

        public static ConfigFileResult<ConfigFileEntryModel> DecodeEntry(string[] content, ref int index)
        {
            for (; index < content.Length; index++)
            {
                var line = content[index];

                if (IsComment(line) || string.IsNullOrWhiteSpace(line))
                    continue;

                if (IsKeyValuePair(line))
                {
                    var keyValuePairResult = DecodeKeyValuePair(line);
                    index++;
                    if (!keyValuePairResult.Success)
                        return ConfigFileResult<ConfigFileEntryModel>.Fail(keyValuePairResult.Errors);

                    var entryResult = new ConfigFileEntryModel
                    {
                        Key = keyValuePairResult.Value.Item1,
                        Value = keyValuePairResult.Value.Item2
                    };

                    return entryResult;
                }
                else
                {
                    index++;
                    return ConfigFileResult<ConfigFileEntryModel>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyValuePair, $"Invalid key-value pair: {line}"));
                }
            }

            return ConfigFileResult<ConfigFileEntryModel>.Fail(new ConfigFileError(ConfigFileErrorCode.EntryNotFound, "No entry found in content"));
        }
    }
}
