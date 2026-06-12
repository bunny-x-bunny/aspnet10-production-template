using System.Security.Claims;
using Domain.Enum;

namespace Common.Extensions {
    public static class ClaimsPrincipialExt {
        public static IEnumerable<Role> Roles(this ClaimsPrincipal principal)
            => principal.FindAll(ClaimTypes.Role).Select(x => System.Enum.Parse<Role>(x.Value));

        public static Guid? Uuid(this ClaimsPrincipal principal)
            //=> UserManager.
             => principal.FindFirst(ClaimTypes.NameIdentifier)?.Value switch {
                 string x => new Guid(x),
                 null => null
             };
    }
}