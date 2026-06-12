using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Enum {
    [JsonConverter(typeof(JsonStringEnumConverter<SortOrder>))]
    public enum SortOrder {
        [Description("📈 По возрастанию")]
        Asc,
        [Description("📉 По убыванию")]
        Desc
    }
}