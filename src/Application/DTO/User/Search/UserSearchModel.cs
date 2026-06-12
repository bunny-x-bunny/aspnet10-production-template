using Common.Enum;
using Common.Extensions;
using Domain.Enum;
using Domain.Models.User;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq.Expressions;

namespace Application.DTO.User.Search {
    public class UserSearchModel {
        [Description("🔍 Email")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string? Email { get; set; }
        public StringOp? EmailOp { get; set; }
        [Description("🔍 Full name")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string? FullName { get; set; }
        public StringOp? FullNameOp { get; set; }
        [Description("🔍 Phone")]
        [DisplayFormat(ConvertEmptyStringToNull = false)]
        public string? PhoneNumber { get; set; }
        public StringOp? PhoneNumberOp { get; set; }
        [Description("🔍 Role")]
        public ICollection<Role>? Role { get; set; }

        public Expression<Func<T, bool>> ToPredicate<T>() where T: AppUser {
            Expression<Func<T, bool>> p = u => true;

            if (Email is not null && EmailOp is StringOp @email_op)
                p = p.StringOp(@email_op, u => u.Email, Email);
            if (FullName is not null && FullNameOp is StringOp @full_name_op)
                p = p.StringOp(full_name_op, u => u.FullName, FullName);
            if (PhoneNumber is not null && PhoneNumberOp is StringOp @phone_number_op)
                p = p.StringOp(@phone_number_op, u => u.PhoneNumber, PhoneNumber);
            if (Role is not null)
                p = p.And(u => Role.Contains(u.Role));
           
            return p;
        }
    }
}
