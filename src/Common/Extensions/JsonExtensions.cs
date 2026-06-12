using System.Text.Json;

namespace Common.Extensions {
    public static class JsonExtensions {
        public static bool IsNull(this JsonDocument json)
            => json is null || json.RootElement.ValueKind is JsonValueKind.Null;
    }
}
