namespace BaseDock.Application.Abstractions.Data;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Session> Sessions { get; }

    DbSet<Project> Projects { get; }

    DbSet<ProjectMember> ProjectMembers { get; }

    DbSet<Environment> Environments { get; }

    DbSet<Service> Services { get; }

    DbSet<Volume> Volumes { get; }

    DbSet<Network> Networks { get; }

    DbSet<Config> Configs { get; }

    DbSet<Secret> Secrets { get; }

    DbSet<ServiceNetwork> ServiceNetworks { get; }

    DbSet<ServiceConfig> ServiceConfigs { get; }

    DbSet<ServiceSecret> ServiceSecrets { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
