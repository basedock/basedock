namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ConfigConfiguration : IEntityTypeConfiguration<Config>
{
    public void Configure(EntityTypeBuilder<Config> builder)
    {
        builder.ToTable("configs");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id");

        builder.Property(c => c.EnvironmentId)
            .HasColumnName("environment_id")
            .IsRequired();

        builder.Property(c => c.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(c => new { c.EnvironmentId, c.Name })
            .IsUnique();

        builder.Property(c => c.Content)
            .HasColumnName("content");

        builder.Property(c => c.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(500);

        builder.Property(c => c.External)
            .HasColumnName("external")
            .HasDefaultValue(false);

        builder.Property(c => c.ExternalName)
            .HasColumnName("external_name")
            .HasMaxLength(255);

        builder.Property(c => c.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
