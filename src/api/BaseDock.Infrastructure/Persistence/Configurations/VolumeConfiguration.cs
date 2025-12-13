namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class VolumeConfiguration : IEntityTypeConfiguration<Volume>
{
    public void Configure(EntityTypeBuilder<Volume> builder)
    {
        builder.ToTable("volumes");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.Property(v => v.EnvironmentId)
            .HasColumnName("environment_id")
            .IsRequired();

        builder.Property(v => v.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(v => new { v.EnvironmentId, v.Name })
            .IsUnique();

        builder.Property(v => v.Driver)
            .HasColumnName("driver")
            .HasMaxLength(100);

        builder.Property(v => v.DriverOpts)
            .HasColumnName("driver_opts")
            .HasColumnType("jsonb");

        builder.Property(v => v.Labels)
            .HasColumnName("labels")
            .HasColumnType("jsonb");

        builder.Property(v => v.External)
            .HasColumnName("external")
            .HasDefaultValue(false);

        builder.Property(v => v.ExternalName)
            .HasColumnName("external_name")
            .HasMaxLength(255);

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
