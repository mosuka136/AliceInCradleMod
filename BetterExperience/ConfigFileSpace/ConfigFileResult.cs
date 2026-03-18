using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BetterExperience.ConfigFileSpace
{
    public class ConfigFileResult<T>
    {
        public T Value { get; private set; }
        public bool Success { get; private set; }
        public IReadOnlyList<ConfigFileError> Errors { get; private set; }

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

        public static implicit operator bool(ConfigFileResult<T> result)
        {
            return result.Success;
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

    public class ConfigFileError
    {
        public ConfigFileErrorCode Code { get; private set; }
        public string Message { get; private set; }
        public string Caller { get; private set; }

        public ConfigFileError(ConfigFileErrorCode code, string message, [CallerMemberName]string caller = "")
        {
            Code = code;
            Message = message;
            Caller = caller;
        }
    }

    public enum ConfigFileErrorCode
    {
        UnsupportedType,
        InvalidValue,
    }
}
