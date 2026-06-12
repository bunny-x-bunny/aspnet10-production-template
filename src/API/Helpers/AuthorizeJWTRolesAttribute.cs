using Domain.Enum;
using Microsoft.AspNetCore.Authorization;

namespace API.Helpers {
    public class AuthorizeJWTRolesAttribute : AuthorizeAttribute {
        public AuthorizeJWTRolesAttribute(params Role[] roles) {
            Roles = string.Join(",", roles);
        }
    }
}
