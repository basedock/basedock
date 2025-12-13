namespace BaseDock.Application.Abstractions.Data;

using BaseDock.Domain.Entities;
using BaseDock.Domain.Entities.Resources;
using Microsoft.EntityFrameworkCore;

public interface IApplicationDbContext
{
    DbSet<User> Users { get; }

    DbSet<Session> Sessions { get; }

    DbSet<Project> Projects { get; }

    DbSet<ProjectMember> ProjectMembers { get; }

    DbSet<Environment> Environments { get; }

    DbSet<EnvironmentVariable> EnvironmentVariables { get; }

    DbSet<DockerImageResource> DockerImageResources { get; }

    DbSet<DockerfileResource> DockerfileResources { get; }

    DbSet<DockerComposeResource> DockerComposeResources { get; }

    DbSet<PostgreSQLResource> PostgreSQLResources { get; }

    DbSet<RedisResource> RedisResources { get; }

    DbSet<PreMadeAppResource> PreMadeAppResources { get; }

    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
