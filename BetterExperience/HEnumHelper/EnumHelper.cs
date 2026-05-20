using System;
using System.Collections.Concurrent;
using System.ComponentModel;

namespace BetterExperience.HEnumHelper
{
    /// <summary>
    /// 枚举元数据读取工具。
    /// 用于读取 Description 和显示控制特性，并缓存反射结果以降低配置界面每帧绘制成本。
    /// </summary>
    public static partial class EnumHelper
    {
        // 枚举字段特性按“枚举类型 + 字段名 + 特性类型”缓存；未标记字段会缓存 null。
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
                return fieldInfo?.GetCustomAttributes(typeof(TAttribute), false) is TAttribute[] attribute && attribute.Length > 0 ? attribute[0] : null;
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
