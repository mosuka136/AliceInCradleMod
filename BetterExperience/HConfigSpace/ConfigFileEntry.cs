using BetterExperience.HTranslatorSpace;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static BetterExperience.HConfigSpace.ConfigFileModel;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 表示配置文件中的一个键值项及其写出时附带的说明元数据。
    /// 该类型只维护文件层面的字符串表示，强类型值的校验和运行时事件由 <see cref="ConfigEntry{T}"/> 负责。
    /// </summary>
    public class ConfigFileEntry
    {
        private string _key;

        /// <summary>
        /// 写入配置文件的多语言名称注释；解析已有文件时当前不会从注释中恢复该值。
        /// </summary>
        public Translator Name { get; set; }
        /// <summary>
        /// 写入配置文件的多语言说明注释；解析已有文件时当前不会从注释中恢复该值。
        /// </summary>
        public Translator Description { get; set; }
        /// <summary>
        /// 配置项键名，只允许字母、数字和下划线，以避免与配置文件语法冲突。
        /// </summary>
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
        /// <summary>
        /// 配置文件中的值文本，已经按 <see cref="ConfigFileModel"/> 规则编码。
        /// </summary>
        public string Value { get; set; }
        /// <summary>
        /// 默认值的编码文本，用作配置文件注释，不参与运行时回退逻辑。
        /// </summary>
        public string DefaultValue { get; set; }
        /// <summary>
        /// 配置值类型的说明文本，用于帮助人工编辑配置文件。
        /// </summary>
        public string ValueType { get; set; }
        /// <summary>
        /// 可接受值的说明文本，用于枚举类型。
        /// </summary>
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

        /// <summary>
        /// 将配置项编码为完整文件片段，包含可选注释和必需的键值行。
        /// </summary>
        /// <returns>可直接写入配置文件的文本片段。</returns>
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

        /// <summary>
        /// 将当前项的元数据复制到另一个文件项。
        /// </summary>
        /// <param name="target">目标配置项。</param>
        /// <param name="overrideValue">是否连同当前值一起覆盖目标值；重载配置时通常应为 <c>false</c>，以保留用户编辑。</param>
        /// <returns>目标存在且复制完成时返回 <c>true</c>。</returns>
        public bool CopyTo(ConfigFileEntry target, bool overrideValue)
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

        /// <summary>
        /// 判断键名是否符合配置文件语法约束。
        /// </summary>
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
            if (typeof(IConfigEntryAdapter).IsAssignableFrom(type))
            {
                try
                {
                    var adapterInstance = (IConfigEntryAdapter)Activator.CreateInstance(type);
                    var result = adapterInstance.EncodeValueType();
                    if (!result.Success)
                        return ConfigFileResult<string>.Fail(result.Errors);

                    return result;
                }
                catch (Exception ex)
                {
                    return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidType, $"Failed to encode value type for {type.FullName}: {ex.Message}"));
                }
            }

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

        public static ConfigFileResult<ConfigFileEntry> CreateEntry<T>(string key, T value, T defaultValue, Translator name, Translator description)
        {
            if (!IsValidKeyName(key))
                return ConfigFileResult<ConfigFileEntry>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyName, $"Invalid key name: {key}"));

            var encodedValueResult = EncodeValue(value);
            if (!encodedValueResult.Success)
                return ConfigFileResult<ConfigFileEntry>.Fail(encodedValueResult.Errors);

            var encodedDefaultValueResult = EncodeValue(defaultValue);
            if (!encodedDefaultValueResult.Success)
                return ConfigFileResult<ConfigFileEntry>.Fail(encodedDefaultValueResult.Errors);

            var encodedValueTypeResult = EncodeValueType<T>();
            if (!encodedValueTypeResult.Success)
                return ConfigFileResult<ConfigFileEntry>.Fail(encodedValueTypeResult.Errors);

            var entry = new ConfigFileEntry
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

        /// <summary>
        /// 解析单行键值对。
        /// </summary>
        /// <param name="content">形如 <c>Key = Value</c> 的配置行，值部分可包含额外的等号。</param>
        /// <returns>键和值的原始字符串。</returns>
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

        /// <summary>
        /// 从当前位置开始解析下一个配置项。
        /// </summary>
        /// <param name="content">按行拆分后的配置文件内容。</param>
        /// <param name="index">读取起点；返回时会推进到已消费内容之后。</param>
        /// <returns>解析出的文件项；到达结尾或遇到非法键值行时返回失败。</returns>
        public static ConfigFileResult<ConfigFileEntry> DecodeEntry(string[] content, ref int index)
        {
            // 解析阶段只信任实际键值行；注释用于人工阅读，启动后会由运行时声明重新写入最新元数据。
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
                        return ConfigFileResult<ConfigFileEntry>.Fail(keyValuePairResult.Errors);

                    var entryResult = new ConfigFileEntry
                    {
                        Key = keyValuePairResult.Value.Item1,
                        Value = keyValuePairResult.Value.Item2
                    };

                    return entryResult;
                }
                else
                {
                    index++;
                    return ConfigFileResult<ConfigFileEntry>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidKeyValuePair, $"Invalid key-value pair: {line}"));
                }
            }

            return ConfigFileResult<ConfigFileEntry>.Fail(new ConfigFileError(ConfigFileErrorCode.EntryNotFound, "No entry found in content"));
        }
    }
}
