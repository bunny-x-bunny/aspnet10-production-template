using Domain.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.DTO.User {
    public class UpdateUserCredentialsDTO {
        [Description("Email")]
        [EmailAddress]
        [MaxLength(255)]
        public string? Email { get; set; }
        [Description("User role")]
        public Role? Role { get; set; }
        [Description("Password")]
        [MinLength(8)]
        public string? Password { get; set; }
    }
}
