using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Enum {
    [JsonConverter(typeof(JsonStringEnumConverter<Role>))]
    public enum Role {
        [Description("Риелтор")]
        User,
        [Description("Администратор")]
        Admin,
    }
}