using Domain.Enum;
using System.Text.Json;

namespace Application.DTO.Notification {
    public class GetNotificationDTO : IDisposable {
        public required Guid Id { get; set; }
        public required NotificationType Type { get; set; }
        public JsonDocument Data { get; set; }
        public required bool Read { get; set; }
        public required DateTime CreatedAt { get; set; }

        public static GetNotificationDTO FromEntity(Domain.Models.Notification entity) => new() {
            Id = entity.Id,
            Type = entity.Type,
            Data = entity.Data,
            Read = entity.Read,
            CreatedAt = entity.CreatedAt
        };

        public void Dispose() => Data?.Dispose();
    }
}
 