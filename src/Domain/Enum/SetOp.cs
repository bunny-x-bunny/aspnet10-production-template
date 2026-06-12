using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Common.Enum {
    [JsonConverter(typeof(JsonStringEnumConverter<SetOp>))]
    public enum SetOp {
        Subset,
        Intersect,
        Empty,
        NotEmpty
    }
}
