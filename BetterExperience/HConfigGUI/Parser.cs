using BetterExperience.HLogSpace;
using System;
using System.Globalization;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// GUI 输入值解析工具。
    /// 解析规则面向控件输入，使用不随系统区域变化的格式；配置文件自身的编码/解码由 <c>HConfigSpace</c> 处理。
    /// </summary>
    public static class Parser
    {
        /// <summary>
        /// 将控件输入解析为指定类型。
        /// </summary>
        public static ParseResult<T> Parse<T>(object input)
        {
            var result = Parse(typeof(T), input);
            if (result.Success)
                return (T)result.Value;
            else
                return ParseResult<T>.Fail(result.Errors);
        }

        /// <summary>
        /// 将控件输入解析为运行时类型。
        /// </summary>
        /// <param name="targetType">目标类型，支持枚举和 <see cref="Convert.ChangeType(object, Type, IFormatProvider)"/> 可转换类型。</param>
        /// <param name="input">控件产生的原始值。</param>
        /// <returns>解析结果；失败时包含面向日志/提示的错误字符串。</returns>
        public static ParseResult<object> Parse(Type targetType, object input)
        {
            if (input == null)
            {
                HLog.Debug("Failed to parse config UI value: input is null.");
                return ParseResult<object>.Fail("Input is null.");
            }

            if (targetType == null)
            {
                HLog.Debug("Failed to parse config UI value: target type is null.");
                return ParseResult<object>.Fail("Target type is null.");
            }

            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, input.ToString(), true);
                }
                catch (Exception ex)
                {
                    HLog.Debug($"Failed to parse enum value '{input}' to {targetType.FullName}: {ex.Message}");
                    return ParseResult<object>.Fail($"Failed to parse enum: {ex.Message}");
                }
            }

            try
            {
                var converted = Convert.ChangeType(input, targetType, CultureInfo.InvariantCulture);
                return converted;
            }
            catch (Exception ex)
            {
                HLog.Debug($"Failed to convert value '{input}' to {targetType.FullName}: {ex.Message}");
                return ParseResult<object>.Fail($"Failed to parse input: {ex.Message}");
            }
        }
    }
}
