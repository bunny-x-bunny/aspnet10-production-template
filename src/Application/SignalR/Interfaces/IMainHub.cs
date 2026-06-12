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
        [SignalRMethod(summary: $"🛜🔻 {nameof(New)}", description: @"Новое уведомление.  
            Документация для конкретных типов доступна в [`/Notification/.doc`](#tag/notification/get/Notification/.doc/OrderStale)
        ")]
        Task New(GetNotificationDTO notification);
    }
}
