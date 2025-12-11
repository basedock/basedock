namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using BaseDock.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectConfiguration : IEntityTypeConfiguration<Project>
{
    public void Configure(EntityTypeBuilder<Project> builder)
    {
        builder.ToTable("projects");

        builder.HasKey(p => p.Id);

        builder.Property(p => p.Id)
            .HasColumnName("id");

        builder.Property(p => p.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.HasIndex(p => p.Name)
            .IsUnique();

        builder.Property(p => p.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(p => p.ProjectType)
            .HasColumnName("project_type")
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(ProjectType.ComposeFile);

        builder.Property(p => p.CreatedByUserId)
            .HasColumnName("created_by_user_id")
            .IsRequired();

        builder.Property(p => p.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .HasColumnName("updated_at");

        // Docker Compose configuration
        builder.Property(p => p.ComposeFileContent)
            .HasColumnName("compose_file_content")
            .HasColumnType("text");

        // Docker Image configuration (JSON)
        builder.Property(p => p.DockerImageConfig)
            .HasColumnName("docker_image_config")
            .HasColumnType("jsonb");

        builder.Property(p => p.DeploymentStatus)
            .HasColumnName("deployment_status")
            .HasConversion<string>()
            .HasMaxLength(50)
            .HasDefaultValue(DeploymentStatus.NotDeployed);

        builder.Property(p => p.LastDeployedAt)
            .HasColumnName("last_deployed_at");

        builder.Property(p => p.LastDeploymentError)
            .HasColumnName("last_deployment_error")
            .HasMaxLength(2000);

        // Relationships
        builder.HasOne(p => p.CreatedBy)
            .WithMany()
            .HasForeignKey(p => p.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(p => p.Members)
            .WithOne(pm => pm.Project)
            .HasForeignKey(pm => pm.ProjectId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
