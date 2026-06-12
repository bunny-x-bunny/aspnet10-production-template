namespace Persistence.Seeders {
    public interface ISeeder {
        static abstract Task seed(AppDbContext context, IServiceProvider sp);
    }
}
