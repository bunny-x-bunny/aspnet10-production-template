using Microsoft.EntityFrameworkCore;

namespace Persistence.Seeders {
  public class TriggerSeeder : ISeeder {
    public static async Task seed(AppDbContext context, IServiceProvider sp) {
      await context.Database.ExecuteSqlAsync($@"
      
      ");
    }
  }
}
