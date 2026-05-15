using System;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI
{
    /// <summary>
    /// GUI 输入解析结果。
    /// 失败结果携带字符串错误，便于直接用于日志或 UI 提示；它不承担配置文件解析错误分类。
    /// </summary>
    /// <typeparam name="T">解析成功后的值类型。</typeparam>
    public class ParseResult<T>
    {
        /// <summary>
        /// 解析成功时的值；失败时为默认值。
        /// </summary>
        public T Value { get; }
        /// <summary>
        /// 是否解析成功。
        /// </summary>
        public bool Success { get; }
        /// <summary>
        /// 失败说明集合；成功时为空集合。
        /// </summary>
        public IReadOnlyList<string> Errors { get; }

        public ParseResult(T value, bool success, IReadOnlyList<string> errors)
        {
            Value = value;
            Success = success;
            Errors = errors;
        }

        public static ParseResult<T> Ok(T value)
        {
            return new ParseResult<T>(value, true, Array.Empty<string>());
        }

        public static ParseResult<T> Fail(IReadOnlyList<string> errors)
        {
            return new ParseResult<T>(default, false, errors);
        }

        public static ParseResult<T> Fail(params string[] errors)
        {
            return new ParseResult<T>(default, false, errors);
        }

        public static implicit operator ParseResult<T>(T value)
        {
            return Ok(value);
        }

        public static explicit operator T(ParseResult<T> result)
        {
            if (!result.Success)
                throw new InvalidOperationException("Cannot convert a failed ParseResult to its value.");
            return result.Value;
        }
    }
}
