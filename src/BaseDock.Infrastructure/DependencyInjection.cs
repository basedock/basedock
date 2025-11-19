using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace BaseDock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Register DbContext
        // services.AddDbContext<ApplicationDbContext>(options =>
        // {
        //     options.UseNpgsql(configuration.GetConnectionString("basedockdb"));
        // });

        // Register repositories
        // services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        return services;
    }
}
