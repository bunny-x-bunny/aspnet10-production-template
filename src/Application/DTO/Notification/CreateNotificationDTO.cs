using Domain.Enum;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Application.DTO.Notification {
    public class CreateNotificationDTO {
        public required Guid User { get; set; }
        public required NotificationType Type { get; set; }
        public required object Data { get; set; }

        public Domain.Models.Notification ToEntity() => new() {
            UserId = User,
            Type = Type,
            Data = JsonSerializer.SerializeToDocument(Data, new JsonSerializerOptions {
              Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            })
        };
    }
}
