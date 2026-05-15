using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 配置文件值的编码/解码工具。
    /// 该类型定义了项目内部配置文本格式的基础规则：字符串带双引号并转义，集合使用方括号和逗号分隔，数字使用不随系统区域变化的格式。
    /// 它只处理单个值及集合值，不解析表头、键名或注释。
    /// </summary>
    public class ConfigFileModel
    {
        /// <summary>
        /// 将强类型值编码为配置文件中的文本表示。
        /// </summary>
        /// <typeparam name="T">待编码值的静态类型。</typeparam>
        /// <param name="value">待编码值，当前格式不支持 <c>null</c>。</param>
        /// <returns>编码结果；不支持的类型会返回失败结果。</returns>
        public static ConfigFileResult<string> Encode<T>(T value)
        {
            return Encode(value, typeof(T));
        }

        /// <summary>
        /// 按指定类型将对象编码为配置文本。
        /// </summary>
        /// <param name="value">待编码对象。</param>
        /// <param name="type">用于选择编码规则的类型。</param>
        /// <returns>编码后的字符串，或包含错误信息的失败结果。</returns>
        public static ConfigFileResult<string> Encode(object value, Type type)
        {
            if (value == null)
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Value cannot be null"));

            if (type == null)
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Type cannot be null"));

            if (typeof(IConfigEntryAdapter).IsAssignableFrom(type))
            {
                try
                {
                    var adapter = (IConfigEntryAdapter)value;
                    var result = adapter.Encode();
                    if (!result.Success)
                        return ConfigFileResult<string>.Fail(result.Errors);

                    return ConfigFileResult<string>.Ok(result.Value);
                }
                catch (Exception ex)
                {
                    return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to encode using IConfigEntryAdapter. Error: {ex.Message}"));
                }
            }

            switch (type)
            {
                case Type t when t == typeof(string):
                    return ConfigFileResult<string>.Ok(EncodeString((string)value));
                case Type t when t == typeof(sbyte):
                    return ConfigFileResult<string>.Ok(((sbyte)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(short):
                    return ConfigFileResult<string>.Ok(((short)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(int):
                    return ConfigFileResult<string>.Ok(((int)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(long):
                    return ConfigFileResult<string>.Ok(((long)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(byte):
                    return ConfigFileResult<string>.Ok(((byte)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(ushort):
                    return ConfigFileResult<string>.Ok(((ushort)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(uint):
                    return ConfigFileResult<string>.Ok(((uint)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(ulong):
                    return ConfigFileResult<string>.Ok(((ulong)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(float):
                    return ConfigFileResult<string>.Ok(((float)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(double):
                    return ConfigFileResult<string>.Ok(((double)value).ToString(CultureInfo.InvariantCulture));
                case Type t when t == typeof(bool):
                    return ConfigFileResult<string>.Ok(((bool)value).ToString(CultureInfo.InvariantCulture));
                default:
                    break;
            }

            if (type.IsEnum)
                return ConfigFileResult<string>.Ok(value.ToString());

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var collectionType = GetCollectionElementType(type);
                if (collectionType == null)
                    return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Unsupported collection type"));

                var elements = new List<string>();
                foreach (var item in (IEnumerable)value)
                {
                    var result = Encode(item, collectionType);
                    if (!result.Success)
                        return ConfigFileResult<string>.Fail(result.Errors);
                    elements.Add(result.Value);
                }

                // 集合元素本身可能是带引号字符串或嵌套集合，因此分隔符只在解码阶段按状态机处理。
                return ConfigFileResult<string>.Ok($"[{string.Join(",", elements)}]");
            }

            return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Unsupported type"));
        }

        /// <summary>
        /// 将配置文本解码为指定类型。
        /// </summary>
        /// <typeparam name="T">目标类型。</typeparam>
        /// <param name="value">配置文件中的值文本。</param>
        /// <returns>解码后的强类型值。</returns>
        public static ConfigFileResult<T> Decode<T>(string value)
        {
            var result = Decode(value, typeof(T));
            if (!result.Success)
                return ConfigFileResult<T>.Fail(result.Errors);

            if (result.Value is T typedValue)
                return ConfigFileResult<T>.Ok(typedValue);

            return ConfigFileResult<T>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Decoded value cannot be converted to {typeof(T).FullName}"));
        }

        /// <summary>
        /// 按运行时类型解码配置文本。
        /// </summary>
        /// <param name="value">配置文件中的值文本。</param>
        /// <param name="type">目标类型。</param>
        /// <returns>解码后的对象，或包含错误信息的失败结果。</returns>
        public static ConfigFileResult<object> Decode(string value, Type type)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Value cannot be null or whitespace"));

            if (type == null)
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Type cannot be null"));

            value = value.Trim();

            if (typeof(IConfigEntryAdapter).IsAssignableFrom(type))
            {
                try
                {
                    var adapterInstance = (IConfigEntryAdapter)Activator.CreateInstance(type);
                    var result = adapterInstance.Decode(value);
                    if (!result.Success)
                        return ConfigFileResult<object>.Fail(result.Errors);
                    return ConfigFileResult<object>.Ok(result.Value);
                }
                catch (Exception ex)
                {
                    return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to decode using IConfigEntryAdapter. Error: {ex.Message}"));
                }
            }

            switch (type)
            {
                case Type t when t == typeof(string):
                    return DecodeString(value);
                case Type t when t == typeof(sbyte):
                    if (sbyte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sbyteResult))
                        return ConfigFileResult<object>.Ok(sbyteResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid sbyte value: {value}"));
                case Type t when t == typeof(short):
                    if (short.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var shortResult))
                        return ConfigFileResult<object>.Ok(shortResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid short value: {value}"));
                case Type t when t == typeof(int):
                    if (int.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var intResult))
                        return ConfigFileResult<object>.Ok(intResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid int value: {value}"));
                case Type t when t == typeof(long):
                    if (long.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var longResult))
                        return ConfigFileResult<object>.Ok(longResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid long value: {value}"));
                case Type t when t == typeof(byte):
                    if (byte.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var byteResult))
                        return ConfigFileResult<object>.Ok(byteResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid byte value: {value}"));
                case Type t when t == typeof(ushort):
                    if (ushort.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ushortResult))
                        return ConfigFileResult<object>.Ok(ushortResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid ushort value: {value}"));
                case Type t when t == typeof(uint):
                    if (uint.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var uintResult))
                        return ConfigFileResult<object>.Ok(uintResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid uint value: {value}"));
                case Type t when t == typeof(ulong):
                    if (ulong.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var ulongResult))
                        return ConfigFileResult<object>.Ok(ulongResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid ulong value: {value}"));
                case Type t when t == typeof(float):
                    if (float.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var floatResult))
                        return ConfigFileResult<object>.Ok(floatResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid float value: {value}"));
                case Type t when t == typeof(double):
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out var doubleResult))
                        return ConfigFileResult<object>.Ok(doubleResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid double value: {value}"));
                case Type t when t == typeof(bool):
                    if (bool.TryParse(value, out var boolResult))
                        return ConfigFileResult<object>.Ok(boolResult);
                    else
                        return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid bool value: {value}"));
                default:
                    break;
            }

            if (type.IsEnum)
            {
                try
                {
                    var enumValue = Enum.Parse(type, value, true);
                    return ConfigFileResult<object>.Ok(enumValue);
                }
                catch (Exception ex)
                {
                    return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid enum value: {value}. Error: {ex.Message}"));
                }
            }

            if (typeof(IEnumerable).IsAssignableFrom(type))
            {
                var collectionType = GetCollectionElementType(type);
                if (collectionType == null)
                    return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Unsupported collection type"));

                var splitResult = SplitCollectionString(value);
                if (!splitResult.Success)
                    return ConfigFileResult<object>.Fail(splitResult.Errors);

                var elements = new List<object>();
                foreach (var item in splitResult.Value)
                {
                    var result = Decode(item.Trim(), collectionType);
                    if (!result.Success)
                        return ConfigFileResult<object>.Fail(result.Errors);
                    elements.Add(result.Value);
                }

                return CreateCollectionResult(type, collectionType, elements);
            }

            return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Decoding not implemented"));
        }

        /// <summary>
        /// 获取集合类型的元素类型。
        /// </summary>
        /// <param name="collectionType">数组、<see cref="IEnumerable{T}"/> 或实现泛型 IEnumerable 的类型。</param>
        /// <returns>元素类型；无法识别时返回 <c>null</c>。</returns>
        public static Type GetCollectionElementType(Type collectionType)
        {
            if (collectionType.IsArray)
                return collectionType.GetElementType();

            if (collectionType.IsGenericType && collectionType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                return collectionType.GetGenericArguments()[0];

            var enumerableType = collectionType.GetInterfaces()
                .FirstOrDefault(i => i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>));

            return enumerableType?.GetGenericArguments()[0];
        }

        /// <summary>
        /// 按配置格式编码字符串。
        /// </summary>
        /// <param name="value">待编码字符串。</param>
        /// <param name="quote">是否用双引号包裹。</param>
        /// <param name="trim">是否在编码前去掉首尾空白。</param>
        /// <param name="escape">是否转义反斜杠、双引号和常见控制字符。</param>
        /// <returns>编码后的字符串。</returns>
        public static string EncodeString(string value, bool quote = true, bool trim = false, bool escape = true)
        {
            if (trim)
                value = value.Trim();
            if (escape)
                value = value.Replace("\\", "\\\\").Replace("\"", "\\\"").Replace("\n", "\\n").Replace("\r", "\\r").Replace("\t", "\\t");
            if (quote)
                value = $"\"{value}\"";
            return value;
        }

        /// <summary>
        /// 按配置格式解码字符串。
        /// </summary>
        /// <param name="value">待解码文本。</param>
        /// <param name="quote">是否要求文本以双引号包裹。</param>
        /// <param name="trim">是否在解码前去掉首尾空白。</param>
        /// <param name="escape">是否解析反斜杠转义。</param>
        /// <returns>解码后的字符串。</returns>
        public static ConfigFileResult<string> DecodeString(string value, bool quote = true, bool trim = true, bool escape = true)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ConfigFileResult<string>.Ok(string.Empty);

            if (trim)
                value = value.Trim();

            if (quote)
            {
                if (value.StartsWith("\"") && value.EndsWith("\""))
                    value = value.Substring(1, value.Length - 2);
                else
                    return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "String must start and end with a quote"));
            }

            if (escape)
            {
                var unescapeResult = UnescapeString(value);
                if (!unescapeResult.Success)
                    return unescapeResult;

                value = unescapeResult.Value;
            }

            return ConfigFileResult<string>.Ok(value);
        }

        /// <summary>
        /// 将集合文本拆分为元素文本。
        /// </summary>
        /// <param name="value">形如 <c>[a,b]</c> 的集合文本。</param>
        /// <returns>元素文本数组；引号未闭合、括号不平衡或存在空元素时返回失败。</returns>
        public static ConfigFileResult<string[]> SplitCollectionString(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Value cannot be null or whitespace"));

            value = value.Trim();
            if (!value.StartsWith("[") || !value.EndsWith("]"))
                return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Collection string must start with '[' and end with ']'"));

            value = value.Substring(1, value.Length - 2).Trim();
            if (value.Length == 0)
                return ConfigFileResult<string[]>.Ok(new string[0]);

            var elements = new List<string>();
            var currentElement = new StringBuilder();
            bool inQuotes = false;
            int bracketDepth = 0;

            // 不能直接用 Split(',')：字符串元素可能含有转义字符，集合元素也允许嵌套方括号。
            for (int i = 0; i < value.Length; i++)
            {
                char c = value[i];

                if (inQuotes)
                {
                    if (c == '\\')
                    {
                        if (i + 1 >= value.Length)
                            return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Invalid escape sequence at end of string"));

                        char next = value[i + 1];
                        if (next != '\\' && next != '"' && next != 'n' && next != 'r' && next != 't')
                            return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid escape sequence: \\{next}"));

                        currentElement.Append(c);
                        currentElement.Append(next);
                        i++;
                        continue;
                    }

                    if (c == '"')
                    {
                        inQuotes = false;
                        currentElement.Append(c);
                        continue;
                    }

                    currentElement.Append(c);
                    continue;
                }

                if (c == '"')
                {
                    inQuotes = true;
                    currentElement.Append(c);
                    continue;
                }

                if (c == '[')
                {
                    bracketDepth++;
                    currentElement.Append(c);
                    continue;
                }

                if (c == ']')
                {
                    if (bracketDepth == 0)
                        return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Unexpected closing bracket in collection"));

                    bracketDepth--;
                    currentElement.Append(c);
                    continue;
                }

                if (c == ',' && bracketDepth == 0)
                {
                    var element = currentElement.ToString().Trim();
                    if (element.Length == 0)
                        return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Collection contains empty element"));

                    elements.Add(element);
                    currentElement.Clear();
                    continue;
                }

                currentElement.Append(c);
            }

            if (inQuotes)
                return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Unclosed quoted string in collection"));

            if (bracketDepth != 0)
                return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Unbalanced nested collection brackets"));

            var lastElement = currentElement.ToString().Trim();
            if (lastElement.Length == 0)
                return ConfigFileResult<string[]>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Collection contains empty element"));

            elements.Add(lastElement);

            return ConfigFileResult<string[]>.Ok(elements.ToArray());
        }

        /// <summary>
        /// 根据目标集合类型创建解码结果。
        /// 支持数组、可由数组/List 构造的类型，以及有无参构造函数并提供兼容 <c>Add</c> 方法的集合类型。
        /// </summary>
        /// <param name="type">目标集合类型。</param>
        /// <param name="elementType">集合元素类型。</param>
        /// <param name="elements">已解码但尚未放入目标集合的元素。</param>
        /// <returns>目标集合实例。</returns>
        public static ConfigFileResult<object> CreateCollectionResult(Type type, Type elementType, List<object> elements)
        {
            if (type == null)
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Type cannot be null"));

            if (elementType == null)
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Element type cannot be null"));

            if (elements == null)
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Elements cannot be null"));

            var validatedElementsResult = ValidateCollectionElements(elementType, elements);
            if (!validatedElementsResult.Success)
                return ConfigFileResult<object>.Fail(validatedElementsResult.Errors);

            var arrayResult = CreateTypedArray(elementType, validatedElementsResult.Value);
            if (!arrayResult.Success)
                return ConfigFileResult<object>.Fail(arrayResult.Errors);

            if (type.IsArray)
                return ConfigFileResult<object>.Ok(arrayResult.Value);

            var listResult = CreateTypedList(elementType, validatedElementsResult.Value);
            if (!listResult.Success)
                return ConfigFileResult<object>.Fail(listResult.Errors);

            var list = listResult.Value;
            var listType = list.GetType();
            if (type.IsAssignableFrom(listType))
                return ConfigFileResult<object>.Ok(list);

            ConfigFileResult<object> constructorResult;
            if (TryCreateCollectionFromConstructor(type, list, arrayResult.Value, out constructorResult))
                return constructorResult;

            ConfigFileResult<object> addMethodResult;
            if (TryCreateCollectionFromAddMethod(type, elementType, validatedElementsResult.Value, out addMethodResult))
                return addMethodResult;

            return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, $"Unsupported collection type: {type.FullName}"));
        }

        /// <summary>
        /// 校验集合元素是否能安全放入目标元素类型。
        /// </summary>
        /// <param name="elementType">集合声明的元素类型。</param>
        /// <param name="elements">已解码元素。</param>
        /// <returns>校验后的元素数组。</returns>
        public static ConfigFileResult<object[]> ValidateCollectionElements(Type elementType, List<object> elements)
        {
            var validatedElements = new object[elements.Count];
            for (int i = 0; i < elements.Count; i++)
            {
                var validationResult = ValidateCollectionElement(elementType, elements[i], i);
                if (!validationResult.Success)
                    return ConfigFileResult<object[]>.Fail(validationResult.Errors);

                validatedElements[i] = validationResult.Value;
            }

            return ConfigFileResult<object[]>.Ok(validatedElements);
        }

        /// <summary>
        /// 校验单个集合元素的赋值兼容性。
        /// </summary>
        /// <param name="elementType">集合声明的元素类型。</param>
        /// <param name="element">待校验元素。</param>
        /// <param name="index">元素在集合中的索引，用于错误定位。</param>
        /// <returns>可放入集合的元素值。</returns>
        public static ConfigFileResult<object> ValidateCollectionElement(Type elementType, object element, int index)
        {
            if (element == null)
            {
                var nullableUnderlyingType = Nullable.GetUnderlyingType(elementType);
                if (!elementType.IsValueType || nullableUnderlyingType != null)
                    return ConfigFileResult<object>.Ok(null);

                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Element at index {index} cannot be null for value type {elementType.FullName}"));
            }

            if (elementType.IsInstanceOfType(element))
                return ConfigFileResult<object>.Ok(element);

            var underlyingType = Nullable.GetUnderlyingType(elementType);
            if (underlyingType != null && underlyingType.IsInstanceOfType(element))
                return ConfigFileResult<object>.Ok(element);

            return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Element at index {index} is not assignable to {elementType.FullName}. Actual type: {element.GetType().FullName}"));
        }

        /// <summary>
        /// 使用反射创建指定元素类型的数组。
        /// </summary>
        /// <param name="elementType">数组元素类型。</param>
        /// <param name="elements">数组元素。</param>
        /// <returns>创建出的数组实例。</returns>
        public static ConfigFileResult<Array> CreateTypedArray(Type elementType, object[] elements)
        {
            try
            {
                var array = Array.CreateInstance(elementType, elements.Length);
                for (int i = 0; i < elements.Length; i++)
                    array.SetValue(elements[i], i);

                return ConfigFileResult<Array>.Ok(array);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidCastException || ex is NotSupportedException)
            {
                return ConfigFileResult<Array>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to create array for element type {elementType.FullName}. Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// 创建辅助 <see cref="List{T}"/>，用于后续直接返回或作为构造函数参数。
        /// </summary>
        /// <param name="elementType">列表元素类型。</param>
        /// <param name="elements">列表元素。</param>
        /// <returns>填充完成的泛型列表。</returns>
        public static ConfigFileResult<IList> CreateTypedList(Type elementType, object[] elements)
        {
            var listType = typeof(List<>).MakeGenericType(elementType);

            try
            {
                var list = (IList)Activator.CreateInstance(listType);
                for (int i = 0; i < elements.Length; i++)
                    list.Add(elements[i]);

                return ConfigFileResult<IList>.Ok(list);
            }
            catch (Exception ex) when (ex is MissingMethodException)
            {
                return ConfigFileResult<IList>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, $"Failed to create helper list for element type {elementType.FullName}. Error: {ex.Message}"));
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidCastException || ex is NotSupportedException)
            {
                return ConfigFileResult<IList>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to populate helper list for element type {elementType.FullName}. Error: {ex.Message}"));
            }
        }

        /// <summary>
        /// 尝试通过单参数构造函数创建目标集合类型。
        /// </summary>
        /// <param name="type">目标集合类型。</param>
        /// <param name="list">辅助列表参数。</param>
        /// <param name="array">辅助数组参数。</param>
        /// <param name="result">如果找到了可用构造函数，则返回成功或失败结果；未找到时为 <c>null</c>。</param>
        /// <returns>是否找到了匹配的构造路径。</returns>
        public static bool TryCreateCollectionFromConstructor(Type type, IList list, Array array, out ConfigFileResult<object> result)
        {
            var constructors = type.GetConstructors(BindingFlags.Instance | BindingFlags.Public)
                .Where(c => c.GetParameters().Length == 1)
                .ToArray();

            var listType = list.GetType();
            var arrayType = array.GetType();

            for (int i = 0; i < constructors.Length; i++)
            {
                var constructor = constructors[i];
                var parameterType = constructor.GetParameters()[0].ParameterType;
                object argument = null;
                if (parameterType.IsAssignableFrom(listType))
                    argument = list;
                else if (parameterType.IsAssignableFrom(arrayType))
                    argument = array;
                else
                    continue;

                try
                {
                    result = ConfigFileResult<object>.Ok(constructor.Invoke(new[] { argument }));
                    return true;
                }
                catch (Exception ex) when (ex is TargetInvocationException || ex is ArgumentException || ex is MemberAccessException)
                {
                    var message = ex is TargetInvocationException invocationException && invocationException.InnerException != null
                        ? invocationException.InnerException.Message
                        : ex.Message;
                    result = ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to construct collection type {type.FullName}. Error: {message}"));
                    return true;
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// 尝试通过无参构造函数和 <c>Add</c> 方法创建目标集合类型。
        /// </summary>
        /// <param name="type">目标集合类型。</param>
        /// <param name="elementType">集合元素类型。</param>
        /// <param name="elements">待添加元素。</param>
        /// <param name="result">如果找到了创建路径，则返回成功或失败结果；未找到时为 <c>null</c>。</param>
        /// <returns>是否找到了可执行的创建路径。</returns>
        public static bool TryCreateCollectionFromAddMethod(Type type, Type elementType, object[] elements, out ConfigFileResult<object> result)
        {
            var constructor = type.GetConstructor(Type.EmptyTypes);
            if (constructor == null)
            {
                result = null;
                return false;
            }

            var addMethod = FindAddMethod(type, elementType);
            if (addMethod == null)
            {
                result = null;
                return false;
            }

            object instance;
            try
            {
                instance = constructor.Invoke(new object[0]);
            }
            catch (Exception ex) when (ex is TargetInvocationException || ex is MemberAccessException)
            {
                var message = ex is TargetInvocationException invocationException && invocationException.InnerException != null
                    ? invocationException.InnerException.Message
                    : ex.Message;
                result = ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to create collection type {type.FullName}. Error: {message}"));
                return true;
            }

            for (int i = 0; i < elements.Length; i++)
            {
                try
                {
                    addMethod.Invoke(instance, new[] { elements[i] });
                }
                catch (Exception ex) when (ex is TargetInvocationException || ex is ArgumentException || ex is TargetParameterCountException || ex is MethodAccessException)
                {
                    var message = ex is TargetInvocationException invocationException && invocationException.InnerException != null
                        ? invocationException.InnerException.Message
                        : ex.Message;
                    result = ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to add element at index {i} to collection type {type.FullName}. Error: {message}"));
                    return true;
                }
            }

            result = ConfigFileResult<object>.Ok(instance);
            return true;
        }

        /// <summary>
        /// 查找可接受指定元素类型的公开实例 <c>Add</c> 方法。
        /// </summary>
        /// <param name="type">集合类型。</param>
        /// <param name="elementType">集合元素类型。</param>
        /// <returns>匹配的方法；不存在时返回 <c>null</c>。</returns>
        public static MethodInfo FindAddMethod(Type type, Type elementType)
        {
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public)
                .Where(m => m.Name == "Add")
                .ToArray();

            for (int i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                var parameters = method.GetParameters();
                if (parameters.Length != 1)
                    continue;

                var parameterType = parameters[0].ParameterType;
                if (parameterType == elementType || parameterType.IsAssignableFrom(elementType))
                    return method;

                var underlyingType = Nullable.GetUnderlyingType(elementType);
                if (underlyingType != null && parameterType.IsAssignableFrom(underlyingType))
                    return method;
            }

            return null;
        }

        /// <summary>
        /// 解析配置字符串中的转义序列。
        /// 仅支持本配置格式写出的反斜杠、双引号、换行、回车和制表符转义。
        /// </summary>
        /// <param name="value">不含外层引号的字符串内容。</param>
        /// <returns>解析后的字符串。</returns>
        public static ConfigFileResult<string> UnescapeString(string value)
        {
            var builder = new StringBuilder(value.Length);
            for (int i = 0; i < value.Length; i++)
            {
                var current = value[i];
                if (current != '\\')
                {
                    builder.Append(current);
                    continue;
                }

                if (i + 1 >= value.Length)
                    return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Invalid escape sequence at end of string"));

                i++;
                switch (value[i])
                {
                    case '\\':
                        builder.Append('\\');
                        break;
                    case '"':
                        builder.Append('"');
                        break;
                    case 'n':
                        builder.Append('\n');
                        break;
                    case 'r':
                        builder.Append('\r');
                        break;
                    case 't':
                        builder.Append('\t');
                        break;
                    default:
                        return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Invalid escape sequence: \\{value[i]}"));
                }
            }

            return ConfigFileResult<string>.Ok(builder.ToString());
        }
    }
}
