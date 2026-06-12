using Application.DTO.Notification;
using SignalRSwaggerGen.Attributes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.SignalR.Interfaces {
    [SignalRHub]
    public interface IMainHub {
        [SignalRMethod(summary: $"🛜🔻 {nameof(New)}", description: @"New notification.
            Documentation for specific types is available at [`/Notification/.doc`](#tag/notification/get/Notification/.doc/OrderStale)
        ")]
        Task New(GetNotificationDTO notification);
    }
}
