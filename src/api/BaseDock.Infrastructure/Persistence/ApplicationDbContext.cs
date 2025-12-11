namespace BaseDock.Infrastructure.Persistence;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Entities.Resources;
using Microsoft.EntityFrameworkCore;

public class ApplicationDbContext : DbContext, IApplicationDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<Session> Sessions => Set<Session>();

    public DbSet<Project> Projects => Set<Project>();

    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();

    public DbSet<Environment> Environments => Set<Environment>();

    public DbSet<EnvironmentVariable> EnvironmentVariables => Set<EnvironmentVariable>();

    public DbSet<DockerImageResource> DockerImageResources => Set<DockerImageResource>();

    public DbSet<DockerfileResource> DockerfileResources => Set<DockerfileResource>();

    public DbSet<DockerComposeResource> DockerComposeResources => Set<DockerComposeResource>();

    public DbSet<PostgreSQLResource> PostgreSQLResources => Set<PostgreSQLResource>();

    public DbSet<RedisResource> RedisResources => Set<RedisResource>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
