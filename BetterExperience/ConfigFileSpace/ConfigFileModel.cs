using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;

namespace BetterExperience.ConfigFileSpace
{
    public class ConfigFileModel
    {
        public static ConfigFileResult<string> Encode<T>(T value)
        {
            return Encode(value, typeof(T));
        }

        public static ConfigFileResult<string> Encode(object value, Type type)
        {
            if (value == null)
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Value cannot be null"));

            if (type == null)
                return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Type cannot be null"));

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

                return ConfigFileResult<string>.Ok($"[{string.Join(",", elements)}]");
            }

            return ConfigFileResult<string>.Fail(new ConfigFileError(ConfigFileErrorCode.UnsupportedType, "Unsupported type"));
        }

        public static ConfigFileResult<T> Decode<T>(string value)
        {
            var result = Decode(value, typeof(T));
            if (!result.Success)
                return ConfigFileResult<T>.Fail(result.Errors);

            if (result.Value is T typedValue)
                return ConfigFileResult<T>.Ok(typedValue);

            return ConfigFileResult<T>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Decoded value cannot be converted to {typeof(T).FullName}"));
        }

        public static ConfigFileResult<object> Decode(string value, Type type)
        {
            if (string.IsNullOrWhiteSpace(value))
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Value cannot be null or whitespace"));

            if (type == null)
                return ConfigFileResult<object>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, "Type cannot be null"));

            value = value.Trim();

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

        public static ConfigFileResult<Array> CreateTypedArray(Type elementType, object[] elements)
        {
            try
            {
                var array = Array.CreateInstance(elementType, elements.Length);
                for (int i = 0; i < elements.Length; i++)
                    array.SetValue(elements[i], i);

                return ConfigFileResult<Array>.Ok(array);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidCastException)
            {
                return ConfigFileResult<Array>.Fail(new ConfigFileError(ConfigFileErrorCode.InvalidValue, $"Failed to create array for element type {elementType.FullName}. Error: {ex.Message}"));
            }
        }

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
