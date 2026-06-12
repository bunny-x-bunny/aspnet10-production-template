using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Domain.Models {
    public class Notification : IDisposable {
        [Key]
        public Guid Id { get; init; }
        [Description("Получатель")]
        public required Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;
        [Description("Тип уведомления")]
        public required NotificationType Type { get; set; }
        [Description("Данные уведомления")]
        public required JsonDocument Data { get; set; }
        [Description("Прочитано")]
        public bool Read { get; set; } = false;
        [Description("Дата создания")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void Dispose() => Data?.Dispose();
    }
}
