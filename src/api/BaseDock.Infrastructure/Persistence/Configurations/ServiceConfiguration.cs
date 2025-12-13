namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using BaseDock.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ServiceConfiguration : IEntityTypeConfiguration<Service>
{
    public void Configure(EntityTypeBuilder<Service> builder)
    {
        builder.ToTable("services");

        builder.HasKey(s => s.Id);

        builder.Property(s => s.Id)
            .HasColumnName("id");

        builder.Property(s => s.EnvironmentId)
            .HasColumnName("environment_id")
            .IsRequired();

        builder.Property(s => s.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(s => s.Slug)
            .HasColumnName("slug")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(s => new { s.EnvironmentId, s.Slug })
            .IsUnique();

        builder.Property(s => s.Description)
            .HasColumnName("description")
            .HasMaxLength(1000);

        // Image or Build
        builder.Property(s => s.Image)
            .HasColumnName("image")
            .HasMaxLength(500);

        builder.Property(s => s.BuildContext)
            .HasColumnName("build_context");

        builder.Property(s => s.BuildDockerfile)
            .HasColumnName("build_dockerfile");

        builder.Property(s => s.BuildArgs)
            .HasColumnName("build_args")
            .HasColumnType("jsonb");

        // Runtime Config
        builder.Property(s => s.Command)
            .HasColumnName("command");

        builder.Property(s => s.Entrypoint)
            .HasColumnName("entrypoint");

        builder.Property(s => s.WorkingDir)
            .HasColumnName("working_dir")
            .HasMaxLength(500);

        builder.Property(s => s.User)
            .HasColumnName("user")
            .HasMaxLength(100);

        // Networking
        builder.Property(s => s.Ports)
            .HasColumnName("ports")
            .HasColumnType("jsonb");

        builder.Property(s => s.Expose)
            .HasColumnName("expose");

        builder.Property(s => s.Hostname)
            .HasColumnName("hostname")
            .HasMaxLength(255);

        builder.Property(s => s.Domainname)
            .HasColumnName("domainname")
            .HasMaxLength(255);

        builder.Property(s => s.Dns)
            .HasColumnName("dns");

        builder.Property(s => s.ExtraHosts)
            .HasColumnName("extra_hosts")
            .HasColumnType("jsonb");

        // Environment Variables
        builder.Property(s => s.EnvironmentVariables)
            .HasColumnName("environment_variables")
            .HasColumnType("jsonb");

        builder.Property(s => s.EnvFile)
            .HasColumnName("env_file");

        // Volumes & Storage
        builder.Property(s => s.Volumes)
            .HasColumnName("volumes")
            .HasColumnType("jsonb");

        builder.Property(s => s.Tmpfs)
            .HasColumnName("tmpfs");

        // Dependencies
        builder.Property(s => s.DependsOn)
            .HasColumnName("depends_on")
            .HasColumnType("jsonb");

        builder.Property(s => s.Links)
            .HasColumnName("links");

        // Health Check
        builder.Property(s => s.HealthcheckTest)
            .HasColumnName("healthcheck_test");

        builder.Property(s => s.HealthcheckInterval)
            .HasColumnName("healthcheck_interval")
            .HasMaxLength(50);

        builder.Property(s => s.HealthcheckTimeout)
            .HasColumnName("healthcheck_timeout")
            .HasMaxLength(50);

        builder.Property(s => s.HealthcheckRetries)
            .HasColumnName("healthcheck_retries");

        builder.Property(s => s.HealthcheckStartPeriod)
            .HasColumnName("healthcheck_start_period")
            .HasMaxLength(50);

        builder.Property(s => s.HealthcheckDisabled)
            .HasColumnName("healthcheck_disabled")
            .HasDefaultValue(false);

        // Resources
        builder.Property(s => s.CpuLimit)
            .HasColumnName("cpu_limit")
            .HasMaxLength(50);

        builder.Property(s => s.MemoryLimit)
            .HasColumnName("memory_limit")
            .HasMaxLength(50);

        builder.Property(s => s.CpuReservation)
            .HasColumnName("cpu_reservation")
            .HasMaxLength(50);

        builder.Property(s => s.MemoryReservation)
            .HasColumnName("memory_reservation")
            .HasMaxLength(50);

        // Lifecycle
        builder.Property(s => s.Restart)
            .HasColumnName("restart")
            .HasMaxLength(50);

        builder.Property(s => s.StopGracePeriod)
            .HasColumnName("stop_grace_period")
            .HasMaxLength(50);

        builder.Property(s => s.StopSignal)
            .HasColumnName("stop_signal")
            .HasMaxLength(50);

        // Deployment
        builder.Property(s => s.Replicas)
            .HasColumnName("replicas")
            .HasDefaultValue(1);

        // Labels
        builder.Property(s => s.Labels)
            .HasColumnName("labels")
            .HasColumnType("jsonb");

        // Status
        builder.Property(s => s.DeploymentStatus)
            .HasColumnName("deployment_status")
            .HasDefaultValue(DeploymentStatus.NotDeployed);

        builder.Property(s => s.LastDeployedAt)
            .HasColumnName("last_deployed_at");

        builder.Property(s => s.LastError)
            .HasColumnName("last_error");

        // Metadata
        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(s => s.UpdatedAt)
            .HasColumnName("updated_at");

        // Navigation properties configured in junction table configurations
    }
}
