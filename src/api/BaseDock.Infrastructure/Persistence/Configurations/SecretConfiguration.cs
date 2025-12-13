namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class SecretConfiguration : IEntityTypeConfiguration<Secret>
{
    public void Configure(EntityTypeBuilder<Secret> builder)
    {
        builder.ToTable("secrets");

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

        builder.HasIndex(s => new { s.EnvironmentId, s.Name })
            .IsUnique();

        builder.Property(s => s.Content)
            .HasColumnName("content");

        builder.Property(s => s.FilePath)
            .HasColumnName("file_path")
            .HasMaxLength(500);

        builder.Property(s => s.External)
            .HasColumnName("external")
            .HasDefaultValue(false);

        builder.Property(s => s.ExternalName)
            .HasColumnName("external_name")
            .HasMaxLength(255);

        builder.Property(s => s.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
