using System;
using System.Globalization;

namespace BetterExperience.HConfigGUI
{
    public static class Parser
    {
        public static ParseResult<T> Parse<T>(object input)
        {
            var result = Parse(typeof(T), input);
            if (result.Success)
                return (T)result.Value;
            else
                return ParseResult<T>.Fail(result.Errors);
        }

        public static ParseResult<object> Parse(Type targetType, object input)
        {
            if (input == null)
                return ParseResult<object>.Fail("Input is null.");

            if (targetType == null)
                return ParseResult<object>.Fail("Target type is null.");

            if (targetType.IsEnum)
            {
                try
                {
                    return Enum.Parse(targetType, input.ToString(), true);
                }
                catch (Exception ex)
                {
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
                return ParseResult<object>.Fail($"Failed to parse input: {ex.Message}");
            }
        }
    }
}
