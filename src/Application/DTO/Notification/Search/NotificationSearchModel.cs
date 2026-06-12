using Common.Enum;
using Common.Extensions;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.Notification.Search {
    public class NotificationSearchModel {
        [Description("🔍 Тип уведомления")]
        public ICollection<NotificationType>? Type { get; set; }
        [Description("🔍 Прочитано")]
        public bool? Read { get; set; }
        [Description("🔍 Дата создания")]
        public DateTime? CreatedAt { get; set; }
        public OrdinalOp? CreatedAtOp { get; set; }

        public Expression<Func<Domain.Models.Notification, bool>> ToPredicate() {
            Expression<Func<Domain.Models.Notification, bool>> p = u => true;

            if (Type is not null)
                p = p.And(n => Type.Contains(n.Type));
            if (Read is bool @read)
                p = p.And(n => n.Read == read);
            if (CreatedAt is DateTime @created_at && CreatedAtOp is OrdinalOp @created_at_op)
                p = p.OrdinalOp(created_at_op, n => n.CreatedAt, created_at);

            return p;
        }
    }
}
