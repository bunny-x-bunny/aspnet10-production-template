using Domain.Enum;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Common.Extensions;
using Persistence;
using Microsoft.EntityFrameworkCore;

namespace API.Helpers {

    public class RoleRequirementFilter(Role[] roles, AppDbContext context) : IAsyncAuthorizationFilter {
        private readonly Role[] _roles = roles;
        private readonly AppDbContext context = context;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext http_context) {
            var user_id = http_context.HttpContext.User.Uuid();

            var user = await context.Users
                .AsNoTracking()
                //.Include(u => u.Roles)
                //.ThenInclude(ur => ur.Role)
                .Where(u => u.Id == user_id)
                .SingleOrDefaultAsync();
            //var roles = user?.Roles.Select(ur => Enum.Parse<Role>(ur.Role.Name!)) ?? [];

            //if (user is null || user.Banned || (_roles.Any() && !roles.Intersect(_roles).Any()))
            if (user is null || !_roles.Contains(user.Role))
                http_context.Result = new StatusCodeResult(403);
        }
    }

    public class AuthorizeDBRolesAttribute : TypeFilterAttribute {
        public AuthorizeDBRolesAttribute(params Role[] roles) : base(typeof(RoleRequirementFilter)) {
            Arguments = [roles];
        }
    }
}
