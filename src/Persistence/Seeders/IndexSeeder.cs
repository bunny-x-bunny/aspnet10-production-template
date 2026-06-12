using Microsoft.EntityFrameworkCore;

namespace Persistence.Seeders {
  public class IndexSeeder : ISeeder {
    public static async Task seed(AppDbContext context, IServiceProvider sp) {
      await context.Database.ExecuteSqlAsync($@"
        create extension if not exists pg_trgm;
    ");
    }
  }
}
