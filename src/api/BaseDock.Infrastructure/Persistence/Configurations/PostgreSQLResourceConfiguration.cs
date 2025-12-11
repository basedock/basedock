namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities.Resources;
using BaseDock.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class PostgreSQLResourceConfiguration : IEntityTypeConfiguration<PostgreSQLResource>
{
    public void Configure(EntityTypeBuilder<PostgreSQLResource> builder)
    {
        builder.ToTable("postgresql_resources");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .HasColumnName("id");

        builder.Property(r => r.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(r => new { r.EnvironmentId, r.Slug })
            .IsUnique();

        builder.Property(r => r.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(r => r.Version)
            .HasColumnName("version")
            .HasMaxLength(20)
            .HasDefaultValue("16");

        builder.Property(r => r.Port)
            .HasColumnName("port")
            .HasDefaultValue(5432);

        builder.Property(r => r.DatabaseName)
            .HasColumnName("database_name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(r => r.Username)
            .HasColumnName("username")
            .HasMaxLength(100)
            .HasDefaultValue("postgres");

        builder.Property(r => r.Password)
            .HasColumnName("password")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(r => r.DeploymentStatus)
            .HasColumnName("deployment_status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(DeploymentStatus.NotDeployed);

        builder.Property(r => r.LastDeployedAt)
            .HasColumnName("last_deployed_at");

        builder.Property(r => r.LastDeploymentError)
            .HasColumnName("last_deployment_error")
            .HasMaxLength(2000);

        builder.Property(r => r.EnvironmentId)
            .HasColumnName("environment_id")
            .IsRequired();

        builder.Property(r => r.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
