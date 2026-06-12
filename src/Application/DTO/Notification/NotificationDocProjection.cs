using Domain.Enum;

namespace Application.DTO.Notification {
    public class NotificationDocProjection<T> {
        public required Guid Id { get; set; }
        public required NotificationType Type { get; set; }
        public required T Data { get; set; }
        public required bool Read { get; set; }
        public required DateTime CreatedAt { get; set; }
    }
}
