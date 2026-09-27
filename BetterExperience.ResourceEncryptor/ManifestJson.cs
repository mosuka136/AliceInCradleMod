using System.Text.Json;

namespace BetterExperience.ResourceEncryptor
{
    // 工具使用 .NET 8 自带解析器，避免引入游戏的 Spine JSON 解析器。
    internal static class ManifestJson
    {
        internal static Dictionary<string, object> Parse(string text)
        {
            using var document = JsonDocument.Parse(text);
            return Object(Convert(document.RootElement));
        }

        private static object Convert(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.Object => value.EnumerateObject().ToDictionary(p => p.Name, p => Convert(p.Value), StringComparer.Ordinal),
            JsonValueKind.Array => value.EnumerateArray().Select(Convert).ToList(),
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number => value.GetDouble(),
            JsonValueKind.True => true,
            JsonValueKind.False => false,
            JsonValueKind.Null => null,
            _ => throw new InvalidDataException("Unsupported JSON value.")
        };

        internal static Dictionary<string, object> Object(object value) => value as Dictionary<string, object>
            ?? throw new InvalidDataException("Expected JSON object.");
        internal static List<object> Array(object value) => value as List<object>
            ?? throw new InvalidDataException("Expected JSON array.");
        internal static object Get(Dictionary<string, object> value, string key) => value.TryGetValue(key, out var result) ? result : null;
        internal static string String(Dictionary<string, object> value, string key, string fallback = null)
        {
            object item = Get(value, key);
            return item == null ? fallback : item as string ?? throw new InvalidDataException("Expected string: " + key);
        }
        internal static int Integer(object value)
        {
            if (value is not double number || !double.IsFinite(number) || number != Math.Truncate(number)
                || number < int.MinValue || number > int.MaxValue) throw new InvalidDataException("Expected integer.");
            return (int)number;
        }
    }
}
