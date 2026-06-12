using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Persistence.Extensions {
    public static class ServiceCollectionExtensions {
        public static IServiceCollection RegisterDataServices(
            this IServiceCollection services, IConfiguration configuration) {
            services.AddDbContextPool<AppDbContext>(opt =>
                opt.UseNpgsql(configuration.GetConnectionString("PostgresConnection")
                    ?? throw new Exception("Connection string \"PostgresConnection\" not found")
                )
            );
            return services;
        }
    }
}
