namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EnvironmentConfiguration : IEntityTypeConfiguration<Environment>
{
    public void Configure(EntityTypeBuilder<Environment> builder)
    {
        builder.ToTable("environments");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id");

        builder.Property(e => e.Name)
            .HasColumnName("name")
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(e => e.Slug)
            .HasColumnName("slug")
            .HasMaxLength(120)
            .IsRequired();

        builder.HasIndex(e => new { e.ProjectId, e.Slug })
            .IsUnique();

        builder.Property(e => e.Description)
            .HasColumnName("description")
            .HasMaxLength(500);

        builder.Property(e => e.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(e => e.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();

        builder.Property(e => e.IsDefault)
            .HasColumnName("is_default")
            .HasDefaultValue(false);

        // Relationships
        builder.HasMany(e => e.Variables)
            .WithOne(v => v.Environment)
            .HasForeignKey(v => v.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DockerImageResources)
            .WithOne(r => r.Environment)
            .HasForeignKey(r => r.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DockerfileResources)
            .WithOne(r => r.Environment)
            .HasForeignKey(r => r.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.DockerComposeResources)
            .WithOne(r => r.Environment)
            .HasForeignKey(r => r.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.PostgreSQLResources)
            .WithOne(r => r.Environment)
            .HasForeignKey(r => r.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(e => e.RedisResources)
            .WithOne(r => r.Environment)
            .HasForeignKey(r => r.EnvironmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
