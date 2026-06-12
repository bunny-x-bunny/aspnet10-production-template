using Common.Enum;
using Common.Extensions;
using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.File {
    public class FileSearchModel {
        [Description("🔍 Создатель")]
        public Guid? User { get; set; }
        [Description("🔍 Имя без расширения")]
        [MaxLength(255)]
        public string? Name { get; set; }
        public StringOp? NameOp { get; set; }
        [Description("🔍 Тип")]
        public ICollection<FileType>? Type { get; set; }
        [Description("🔍 Расширение")]
        public ICollection<string>? Extension { get; set; }
        [Description("🔍 Размер")]
        public long? Size { get; set; }
        public OrdinalOp? SizeOp { get; set; }
        [Description("🔍 Дата создания")]
        public DateTime? CreatedAt { get; set; }
        public OrdinalOp? CreatedAtOp { get; set; }

        public Expression<Func<Domain.Models.File, bool>> ToPredicate() {
            Expression<Func<Domain.Models.File, bool>> p = u => true;

            if (User is not null)
                p = p.And(f => f.UserId == User);
            if (Name is not null && NameOp is StringOp @name_op)
                p = p.StringOp(name_op, p => p.Name, Name);
            if (Type is not null)
                p = p.And(p => Type.Contains(p.Type));
            if (Extension is not null)
                p = p.And(p => Extension.Contains(p.Extension));
            if (Size is long @size && SizeOp is OrdinalOp @size_op)
                p = p.OrdinalOp(size_op, p => p.Size, size);
            if (CreatedAt is DateTime @created_at && CreatedAtOp is OrdinalOp @created_at_op)
                p = p.OrdinalOp(created_at_op, p => p.CreatedAt, created_at);

            return p;
        }
    }
}
