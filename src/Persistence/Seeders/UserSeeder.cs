using Domain.Enum;
using Domain.Models.User;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Seeders {
  public class UserSeeder : ISeeder {
    public static async Task seed(AppDbContext context, IServiceProvider sp) {
      var user_manager = sp.GetRequiredService<UserManager<AppUser>>();

      if (!await context.Users.AnyAsync()) {
        await user_manager.CreateAsync(new AppUser {
          FullName = "Admin Ivanova",
          Role = Role.Admin,
          Email = "admin@example.com",
          UserName = "admin@example.com",
        }, "password");

        await user_manager.CreateAsync(new AppUser {
          FullName = "User Ivanov",
          Role = Role.User,
          Email = "user@example.com",
          UserName = "user@example.com",
        }, "password");
      }
    }
  }
}
