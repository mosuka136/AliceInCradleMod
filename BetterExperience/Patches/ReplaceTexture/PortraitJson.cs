using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace BetterExperience.Patches.ReplaceTexture
{
    // Use the game's JSON reader so numeric values have exactly Spine's runtime precision.
    internal static class PortraitJson
    {
        internal static Dictionary<string, object> Parse(string text)
        {
            using (var reader = new StringReader(text))
                return Object(Spine.Json.Deserialize(reader));
        }

        internal static Dictionary<string, object> Object(object value)
        {
            return value as Dictionary<string, object> ?? throw new InvalidDataException("Expected JSON object.");
        }

        internal static object Get(Dictionary<string, object> value, string key)
        {
            return value.TryGetValue(key, out var result) ? result : null;
        }

        internal static string String(Dictionary<string, object> value, string key, string fallback = null)
        {
            var item = Get(value, key);
            if (item == null)
                return fallback;
            return item as string ?? throw new InvalidDataException("Expected string: " + key);
        }

        internal static List<object> Array(object value)
        {
            return value as List<object> ?? throw new InvalidDataException("Expected JSON array.");
        }

        internal static double Number(object value)
        {
            if (!(value is float || value is double || value is int || value is long))
                throw new InvalidDataException("Expected number.");
            double number = Convert.ToDouble(value, CultureInfo.InvariantCulture);
            if (double.IsInfinity(number) || double.IsNaN(number))
                throw new InvalidDataException("Non-finite number.");
            return number;
        }

        internal static int Integer(object value)
        {
            double number = Number(value);
            if (number != Math.Truncate(number) || number < int.MinValue || number > int.MaxValue)
                throw new InvalidDataException("Expected integer.");
            return (int)number;
        }

        internal static bool Equal(object left, object right)
        {
            return Serialize(left) == Serialize(right);
        }

        internal static string Serialize(object value)
        {
            var output = new StringBuilder();
            Write(output, value);
            return output.ToString();
        }

        private static void Write(StringBuilder output, object value)
        {
            if (value == null) { output.Append("null"); return; }
            if (value is string text)
            {
                output.Append('"');
                foreach (char c in text)
                {
                    if (c == '"' || c == '\\') output.Append('\\').Append(c);
                    else if (c < 32) output.Append("\\u").Append(((int)c).ToString("x4"));
                    else output.Append(c);
                }
                output.Append('"');
                return;
            }
            if (value is bool flag) { output.Append(flag ? "true" : "false"); return; }
            if (value is Dictionary<string, object> map)
            {
                output.Append('{');
                bool first = true;
                foreach (string key in map.Keys.OrderBy(k => k, StringComparer.Ordinal))
                {
                    if (!first) output.Append(',');
                    first = false;
                    Write(output, key);
                    output.Append(':');
                    Write(output, map[key]);
                }
                output.Append('}');
                return;
            }
            if (value is IList list)
            {
                output.Append('[');
                for (int i = 0; i < list.Count; i++)
                {
                    if (i > 0) output.Append(',');
                    Write(output, list[i]);
                }
                output.Append(']');
                return;
            }
            Number(value);
            output.Append(value is float single ? single.ToString("R", CultureInfo.InvariantCulture)
                : Convert.ToString(value, CultureInfo.InvariantCulture));
        }
    }
}
