using Common.Enum;
using Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Persistence.Seeders {
  public class CatSeeder : ISeeder {
    public static async Task seed(AppDbContext context, IServiceProvider sp) {
      if (!await context.Cats.AnyAsync()) {
        var cat1 = new Cat {
          Name = "root",
          LeftEar = 0,
          RightEar = 1,
        };
        context.Cats.Add(cat1);
        await context.SaveChangesAsync();
      }
    }
  }
}
