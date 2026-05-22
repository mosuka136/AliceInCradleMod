using System;
using System.Collections.Generic;

namespace BetterExperience.HotkeyManager
{
    public class HotkeyResult<T>
    {
        /// <summary>
        /// 成功时的返回值；失败时为该类型默认值。
        /// </summary>
        public T Value { get; private set; }
        /// <summary>
        /// 当前结果是否成功。
        /// </summary>
        public bool Success { get; private set; }
        /// <summary>
        /// 失败原因集合；成功时为空集合而不是 <c>null</c>。
        /// </summary>
        public IReadOnlyList<string> Errors { get; private set; }

        public HotkeyResult()
        {
            Errors = Array.Empty<string>();
        }

        public HotkeyResult(T value, bool success, IReadOnlyList<string> errors)
        {
            Value = value;
            Success = success;
            Errors = errors ?? Array.Empty<string>();
        }

        public static HotkeyResult<T> Ok(T value)
        {
            return new HotkeyResult<T>(value, true, Array.Empty<string>());
        }

        public static HotkeyResult<T> Fail(IReadOnlyList<string> errors)
        {
            return new HotkeyResult<T>(default, false, errors);
        }

        public static HotkeyResult<T> Fail(params string[] errors)
        {
            return new HotkeyResult<T>(default, false, errors);
        }

        public static HotkeyResult<T> Fail(string error, params IReadOnlyList<string>[] errors)
        {
            var allErrors = new List<string> { error };
            foreach (var errorList in errors)
            {
                allErrors.AddRange(errorList);
            }
            return new HotkeyResult<T>(default, false, allErrors);
        }

        public static implicit operator HotkeyResult<T>(T value)
        {
            return Ok(value);
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }
    }
}
