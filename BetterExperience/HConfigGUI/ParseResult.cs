using System;
using System.Collections.Generic;

namespace BetterExperience.HConfigGUI
{
    public class ParseResult<T>
    {
        public T Value { get; }
        public bool Success { get; }
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
