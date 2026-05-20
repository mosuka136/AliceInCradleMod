using BetterExperience.HTranslatorSpace;
using System;
using System.Collections;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 运行时配置项的非泛型视图。
    /// UI 层通过该接口读写配置值，而不需要知道具体的泛型类型。
    /// </summary>
    public interface IConfigEntry
    {
        Translator Name { get; }
        Translator Description { get; }
        string TableName { get; }
        string Key { get; }
        ConfigFileEntry Entry { get; }
        Type ValueType { get; }
        object BoxedValue { get; set; }
        object BoxedDefaultValue { get; }

        event EventHandler OnValueChangedBase;

        void RebindEntry(ConfigFileEntry entry);
    }

    /// <summary>
    /// 一个强类型运行时配置项。
    /// 它把文件层面的 <see cref="ConfigFileEntry"/> 与业务代码使用的 <typeparamref name="T"/> 值绑定起来，并在值变化时同步文件项文本与触发事件。
    /// 该类不直接写文件；写回时机由 <see cref="ConfigFileManager"/> 订阅变化事件后决定。
    /// </summary>
    /// <typeparam name="T">配置项值类型。</typeparam>
    public class ConfigEntry<T> : IConfigEntry
    {
        private T _value;

        /// <summary>
        /// 当前配置值。赋值会编码到绑定的文件项，并触发 <see cref="OnValueChanged"/>。
        /// </summary>
        public T Value
        {
            get => _value;
            set
            {
                if (Equal(value, _value))
                    return;

                var valueResult = ConfigFileEntry.EncodeValue(value);
                if (!valueResult.Success)
                {
                    foreach (var error in valueResult.Errors)
                        HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                    throw new InvalidOperationException($"Failed to encode value for key: {Key}, value: {value}. Errors: {string.Join(", ", valueResult.Errors)}");
                }

                _value = value;

                Entry.Value = valueResult.Value;

                try
                {
                    OnValueChanged?.Invoke(this, _value);
                    OnValueChangedBase?.Invoke(this, new EntryValueChangedEventArgs<T>(_value));
                }
                catch (Exception ex)
                {
                    HLog.Error($"Exception in value changed event for key: {Key}, value: {value}.", ex);
                }
            }
        }

        public Translator Name => Entry.Name;
        public Translator Description => Entry.Description;
        public string TableName { get; private set; }
        public string Key => Entry.Key;
        public T DefaultValue { get; private set; }
        public ConfigFileEntry Entry { get; private set; }

        public Type ValueType => typeof(T);

        public object BoxedValue
        {
            get => Value;
            set => Value = (T)value;
        }

        public object BoxedDefaultValue => DefaultValue;

        /// <summary>
        /// 强类型值变化事件。只有新值与旧值不相等时才触发。
        /// </summary>
        public event EventHandler<T> OnValueChanged;

        /// <summary>
        /// 非泛型值变化事件，参数为 <see cref="EntryValueChangedEventArgs{T}"/> 包装的强类型新值。
        /// </summary>
        public event EventHandler OnValueChangedBase;

        public ConfigEntry()
        {

        }

        public ConfigEntry(string tableKey, ConfigFileEntry entry, T defaultValue) :
            this(tableKey, entry, defaultValue, entry.Name, entry.Description)
        {
        }

        public ConfigEntry(string tableKey, ConfigFileEntry entry, T defaultValue, Translator name, Translator description)
        {
            if (entry == null)
                throw new ArgumentNullException(nameof(entry));

            entry.Name = name;
            entry.Description = description;

            var valueTypeResult = ConfigFileEntry.EncodeValueType<T>();
            if (!valueTypeResult.Success)
            {
                foreach (var error in valueTypeResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to encode value type for key: {entry.Key}, type: {typeof(T).FullName}. Errors: {string.Join(", ", valueTypeResult.Errors)}");
            }
            entry.ValueType = valueTypeResult.Value;

            var acceptableValuesResult = ConfigFileEntry.EncodeAcceptableValues<T>();
            if (acceptableValuesResult.Success)
                entry.AcceptableValues = acceptableValuesResult.Value;

            var defaultValueResult = ConfigFileEntry.EncodeValue(defaultValue);
            if (!defaultValueResult.Success)
            {
                foreach (var error in defaultValueResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to encode default value for key: {entry.Key}, value: {defaultValue}. Errors: {string.Join(", ", defaultValueResult.Errors)}");
            }
            entry.DefaultValue = defaultValueResult.Value;

            if (!ConfigFileEntry.IsValidKeyName(entry.Key))
                throw new InvalidOperationException($"Invalid key name: {entry.Key}");

            if (!ConfigFileTable.IsValidTableName(tableKey))
                throw new InvalidOperationException($"Invalid table name: {tableKey}");

            TableName = tableKey;
            DefaultValue = defaultValue;
            RebindEntry(entry);
        }

        /// <summary>
        /// 将配置项重新绑定到另一个文件项，并从文件项的文本值解码当前值。
        /// 常用于重新读取配置文件后保留已有 <see cref="ConfigEntry{T}"/> 引用。
        /// </summary>
        /// <param name="entry">新的文件项；为 <c>null</c> 时不做处理。</param>
        public void RebindEntry(ConfigFileEntry entry)
        {
            if (entry == null)
                return;
            var decodeResult = ConfigFileEntry.DecodeValue<T>(entry.Value);
            if (!decodeResult.Success)
            {
                foreach (var error in decodeResult.Errors)
                    HLog.Error(error.GetFullMessage(), null, string.Empty, string.Empty, 0);
                throw new InvalidOperationException($"Failed to decode value for key: {entry.Key}, value: {entry.Value}. Errors: {string.Join(", ", decodeResult.Errors)}");
            }
            Entry = entry;
            Value = decodeResult.Value;
        }

        /// <summary>
        /// 比较两个配置值是否等价。
        /// 集合类型会按元素顺序深度比较，以避免数组/List 在引用变化但内容未变时触发多余写入。
        /// </summary>
        public static bool Equal(T a, T b)
        {
            return EqualBoxed(a, b);
        }

        /// <summary>
        /// 非泛型等值比较实现。
        /// 当前只支持基础类型、字符串、枚举、数组和 IEnumerable；未知复杂对象按不相等处理。
        /// </summary>
        public static bool EqualBoxed(object a, object b)
        {
            if (a == null && b == null)
                return true;

            if (a == null || b == null)
                return false;

            var type = a.GetType();

            if (type != b.GetType())
                return false;

            if (type.IsPrimitive || type == typeof(string) || type.IsEnum)
                return object.Equals(a, b);

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
                    if (!EqualBoxed(arrayA.GetValue(i), arrayB.GetValue(i)))
                        return false;
                }

                return true;
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var enumA = (a as IEnumerable)?.GetEnumerator();
                var enumB = (b as IEnumerable)?.GetEnumerator();

                if (enumA == null || enumB == null)
                    return false;

                try
                {
                    while (true)
                    {
                        var hasNextA = enumA.MoveNext();
                        var hasNextB = enumB.MoveNext();

                        if (hasNextA != hasNextB)
                            return false;

                        if (!hasNextA)
                            break;

                        if (!EqualBoxed(enumA.Current, enumB.Current))
                            return false;
                    }

                    return true;
                }
                finally
                {
                    (enumA as IDisposable)?.Dispose();
                    (enumB as IDisposable)?.Dispose();
                }
            }

            return false;
        }
    }

    /// <summary>
    /// 通过非泛型事件暴露强类型配置值变化时使用的事件参数。
    /// </summary>
    /// <typeparam name="T">变化后的值类型。</typeparam>
    public class EntryValueChangedEventArgs<T> : EventArgs
    {
        public T Value { get; }

        public EntryValueChangedEventArgs(T value)
        {
            Value = value;
        }

        public static implicit operator T(EntryValueChangedEventArgs<T> args) => args.Value;
    }
}
