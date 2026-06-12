using Domain.Enum;
using Domain.Models.User;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace Domain.Models {
    public class Notification : IDisposable {
        [Key]
        public Guid Id { get; init; }
        [Description("Recipient")]
        public required Guid UserId { get; set; }
        public AppUser User { get; set; } = null!;
        [Description("Notification type")]
        public required NotificationType Type { get; set; }
        [Description("Notification data")]
        public required JsonDocument Data { get; set; }
        [Description("Read")]
        public bool Read { get; set; } = false;
        [Description("Creation date")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public void Dispose() => Data?.Dispose();
    }
}
