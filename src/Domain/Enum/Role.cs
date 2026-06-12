using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Enum {
    [JsonConverter(typeof(JsonStringEnumConverter<Role>))]
    public enum Role {
        [Description("Пользователь")]
        User,
        [Description("Администратор")]
        Admin,
    }
}