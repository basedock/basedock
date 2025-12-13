namespace BaseDock.Infrastructure.Persistence;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Domain.Entities;
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

    public DbSet<Service> Services => Set<Service>();

    public DbSet<Volume> Volumes => Set<Volume>();

    public DbSet<Network> Networks => Set<Network>();

    public DbSet<Config> Configs => Set<Config>();

    public DbSet<Secret> Secrets => Set<Secret>();

    public DbSet<ServiceNetwork> ServiceNetworks => Set<ServiceNetwork>();

    public DbSet<ServiceConfig> ServiceConfigs => Set<ServiceConfig>();

    public DbSet<ServiceSecret> ServiceSecrets => Set<ServiceSecret>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        base.OnModelCreating(modelBuilder);
    }
}
