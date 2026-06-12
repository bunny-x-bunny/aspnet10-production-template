using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Enum {
  [JsonConverter(typeof(JsonStringEnumConverter<FileType>))]
  public enum FileType {
    [Description("User avatar")]
    UserAvatar,
  }
}