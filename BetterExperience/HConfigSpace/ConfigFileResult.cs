using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BetterExperience.HConfigSpace
{
    /// <summary>
    /// 配置文件解析和编码流程使用的轻量结果类型。
    /// 它允许调用方在不抛异常的情况下携带一个值和多个错误；只有严重的使用方式错误才由上层主动抛出异常。
    /// </summary>
    /// <typeparam name="T">成功结果中携带的值类型。</typeparam>
    public class ConfigFileResult<T>
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
        public IReadOnlyList<ConfigFileError> Errors { get; private set; }

        public ConfigFileResult()
        {
            Errors = Array.Empty<ConfigFileError>();
        }

        public ConfigFileResult(T value, bool success, IReadOnlyList<ConfigFileError> errors)
        {
            Value = value;
            Success = success;
            Errors = errors ?? Array.Empty<ConfigFileError>();
        }

        public void SetValue(T value)
        {
            Value = value;
            Success = true;
            if (Errors == null)
                Errors = Array.Empty<ConfigFileError>();
        }

        public void AddError(IReadOnlyList<ConfigFileError> errors)
        {
            var errorList = new List<ConfigFileError>(Errors ?? Array.Empty<ConfigFileError>());
            errorList.AddRange(errors);
            Errors = errorList;
        }

        public void AddError(params ConfigFileError[] errors)
        {
            var errorList = new List<ConfigFileError>(Errors ?? Array.Empty<ConfigFileError>());
            errorList.AddRange(errors);
            Errors = errorList;
        }

        public static ConfigFileResult<T> Ok(T value)
        {
            return new ConfigFileResult<T>
            {
                Value = value,
                Success = true,
                Errors = Array.Empty<ConfigFileError>()
            };
        }

        public static ConfigFileResult<T> Fail(IReadOnlyList<ConfigFileError> errors)
        {
            return new ConfigFileResult<T>
            {
                Value = default,
                Success = false,
                Errors = errors ?? Array.Empty<ConfigFileError>()
            };
        }

        public static ConfigFileResult<T> Fail(params ConfigFileError[] errors)
        {
            return new ConfigFileResult<T>
            {
                Value = default,
                Success = false,
                Errors = errors ?? Array.Empty<ConfigFileError>()
            };
        }

        public override string ToString()
        {
            return Value?.ToString() ?? string.Empty;
        }

        public static implicit operator ConfigFileResult<T>(T value)
        {
            return Ok(value);
        }

        public static explicit operator T(ConfigFileResult<T> result)
        {
            if (!result.Success)
                throw new InvalidOperationException("Cannot convert a failed ConfigFileResult to its value.");

            return result.Value;
        }

        public static implicit operator ConfigFileResult<object>(ConfigFileResult<T> result)
        {
            return new ConfigFileResult<object>
            {
                Value = result.Value,
                Success = result.Success,
                Errors = result.Errors ?? Array.Empty<ConfigFileError>()
            };
        }
    }

    /// <summary>
    /// 配置文件处理中的单个错误。
    /// </summary>
    public class ConfigFileError
    {
        /// <summary>
        /// 机器可判定的错误类型。
        /// </summary>
        public ConfigFileErrorCode Code { get; private set; }
        /// <summary>
        /// 面向日志的错误说明。
        /// </summary>
        public string Message { get; private set; }
        /// <summary>
        /// 创建错误对象的调用成员名，用于快速定位错误来源。
        /// </summary>
        public string Caller { get; private set; }

        public ConfigFileError(ConfigFileErrorCode code, string message, [CallerMemberName]string caller = "")
        {
            Code = code;
            Message = message;
            Caller = caller;
        }

        public override string ToString()
        {
            return $"[{Code}] {Message} (Caller: {Caller})";
        }

        public string GetFullMessage()
        {
            return $"[{Code}] {Message} (Caller: {Caller})";
        }
    }

    /// <summary>
    /// 配置文件解析/编码阶段可识别的错误类别。
    /// </summary>
    public enum ConfigFileErrorCode
    {
        UnsupportedType,
        InvalidValue,
        InvalidKeyValuePair,
        InvalidKeyName,
        InvalidTableName,
        EntryNotFound,
        TableNotFound,
        InvalidTableHeader,
        EndOfContent,
        InvalidType,
    }
}
