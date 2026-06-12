using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Enum {
  [JsonConverter(typeof(JsonStringEnumConverter<NotificationType>))]
  public enum NotificationType {
    [Description("New user registration")]
    UserRegistered,
  }
}