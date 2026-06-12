using Microsoft.EntityFrameworkCore;

namespace Persistence.Seeders {
  public class CommonSeeder : ISeeder {
    public static async Task seed(AppDbContext context, IServiceProvider sp) {
      await context.Database.ExecuteSqlAsync($@"

      ");
    }
  }
}
