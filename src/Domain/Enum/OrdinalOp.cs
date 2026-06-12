using System.Text.Json.Serialization;

namespace Common.Enum {
    [JsonConverter(typeof(JsonStringEnumConverter<OrdinalOp>))]
    public enum OrdinalOp {
        GreaterThan,
        GreaterThanOrEqual,
        Equal,
        LessThanOrEqual,
        LessThan
    }
}
