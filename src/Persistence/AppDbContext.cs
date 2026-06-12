using Domain.Models;
using Domain.Models.User;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Persistence.Seeders;
using System.Text.Encodings.Web;
using System.Text.Json;

namespace Persistence {
  public class AppDbContext : IdentityUserContext<
      AppUser,
      Guid,
      IdentityUserClaim<Guid>,
      IdentityUserLogin<Guid>,
      IdentityUserToken<Guid>
  > {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) {

    }

    public DbSet<Domain.Models.File> Files { get; set; }
    public DbSet<Notification> Notifications { get; set; }


    protected override void OnModelCreating(ModelBuilder builder) {
      base.OnModelCreating(builder);

      builder.Entity<AppUser>(u => {
        u.HasOne(u => u.Avatar).WithMany().OnDelete(DeleteBehavior.SetNull);
      });

      builder.Entity<Domain.Models.File>(b => {
        b.HasOne(b => b.User).WithMany().OnDelete(DeleteBehavior.SetNull);
      });
    }

    public static async Task Seed(IServiceProvider sp, IWebHostEnvironment env) {
      var context = sp.GetRequiredService<AppDbContext>();
      await CommonSeeder.seed(context, sp);
      await StoredFunctionSeeder.seed(context, sp);
      await TriggerSeeder.seed(context, sp);
      await IndexSeeder.seed(context, sp);

      if (env.IsDevelopment() || env.IsStaging()) {
        await UserSeeder.seed(context, sp);
      }
    }
  }
}
