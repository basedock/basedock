namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities.Resources;
using BaseDock.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class DockerComposeResourceConfiguration : IEntityTypeConfiguration<DockerComposeResource>
{
    public void Configure(EntityTypeBuilder<DockerComposeResource> builder)
    {
        builder.ToTable("docker_compose_resources");

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

        builder.Property(r => r.ComposeFileContent)
            .HasColumnName("compose_file_content")
            .HasColumnType("text")
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
