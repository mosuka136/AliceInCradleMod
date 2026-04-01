using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace BetterExperience.HEnumHelper
{
    public static partial class EnumHelper
    {
        private static readonly ConcurrentDictionary<(Type enumType, string enumName, Type attrType), Attribute> _enumValues = new ConcurrentDictionary<(Type, string, Type), Attribute>();

        public static TAttribute GetAttribute<TEnum, TAttribute>(TEnum enumValue) where TEnum : Enum where TAttribute : Attribute
        {
            return GetAttribute<TAttribute>(typeof(TEnum), enumValue);
        }

        public static TAttribute GetAttribute<TAttribute>(Type enumType, Enum enumValue) where TAttribute : Attribute
        {
            var key = (enumType, enumValue.ToString(), typeof(TAttribute));
            return _enumValues.GetOrAdd(key, k =>
            {
                var fieldInfo = k.enumType.GetField(k.enumName);
                var attribute = fieldInfo.GetCustomAttributes(typeof(TAttribute), false) as TAttribute[];
                return attribute != null && attribute.Length > 0 ? attribute[0] : null;
            }) as TAttribute;
        }

        public static string GetDescription<TEnum>(TEnum value) where TEnum : Enum
        {
            return GetAttribute<TEnum, DescriptionAttribute>(value)?.Description ?? value.ToString();
        }

        public static string GetDescription(Type enumType, Enum value)
        {
            return GetAttribute<DescriptionAttribute>(enumType, value)?.Description ?? value.ToString();
        }

        public static bool IsDisplay<TEnum>(TEnum value) where TEnum : Enum
        {
            return GetAttribute<TEnum, DisplayEnumAttribute>(value)?.IsDisplay ?? true;
        }

        public static bool IsDisplay(Type enumType, Enum value)
        {
            return GetAttribute<DisplayEnumAttribute>(enumType, value)?.IsDisplay ?? true;
        }
    }
}
