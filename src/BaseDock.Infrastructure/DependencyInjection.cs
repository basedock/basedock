using BaseDock.Application.Common.Interfaces;
using BaseDock.Infrastructure.Persistence;
using BaseDock.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BaseDock.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services)
    {
        services.AddScoped<IDockerService, DockerService>();
        return services;
    }
}
