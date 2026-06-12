using Microsoft.VisualBasic.FileIO;
using System.ComponentModel;
using System.Text.Json.Serialization;

namespace Domain.Enum {
  [JsonConverter(typeof(JsonStringEnumConverter<FileType>))]
  public enum FileType {
    [Description("Аватар пользователя")]
    UserAvatar,
    [Description("Фото или видео объекта недвижимости")]
    EstateImage,
    [Description("Планировка и документы объекта недвижимости")]
    EstateDocument
  }
}