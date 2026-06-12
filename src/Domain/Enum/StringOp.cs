using System.Text.Json.Serialization;

namespace Common.Enum {
    [JsonConverter(typeof(JsonStringEnumConverter<StringOp>))]
    public enum StringOp {
        Equals,
        NotEquals,
        Contains,
        NotContains,
        StartsWith,
        EndsWith,
        IsEmpty,
        IsNotEmpty
    }
}
