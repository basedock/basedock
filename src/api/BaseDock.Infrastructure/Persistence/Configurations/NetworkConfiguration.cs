namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class NetworkConfiguration : IEntityTypeConfiguration<Network>
{
    public void Configure(EntityTypeBuilder<Network> builder)
    {
        builder.ToTable("networks");

        builder.HasKey(n => n.Id);

        builder.Property(n => n.Id)
            .HasColumnName("id");

        builder.Property(n => n.EnvironmentId)
            .HasColumnName("environment_id")
            .IsRequired();

        builder.Property(n => n.Name)
            .HasColumnName("name")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(n => new { n.EnvironmentId, n.Name })
            .IsUnique();

        builder.Property(n => n.Driver)
            .HasColumnName("driver")
            .HasMaxLength(100);

        builder.Property(n => n.DriverOpts)
            .HasColumnName("driver_opts")
            .HasColumnType("jsonb");

        builder.Property(n => n.IpamDriver)
            .HasColumnName("ipam_driver")
            .HasMaxLength(100);

        builder.Property(n => n.IpamConfig)
            .HasColumnName("ipam_config")
            .HasColumnType("jsonb");

        builder.Property(n => n.Internal)
            .HasColumnName("internal")
            .HasDefaultValue(false);

        builder.Property(n => n.Attachable)
            .HasColumnName("attachable")
            .HasDefaultValue(false);

        builder.Property(n => n.Labels)
            .HasColumnName("labels")
            .HasColumnType("jsonb");

        builder.Property(n => n.External)
            .HasColumnName("external")
            .HasDefaultValue(false);

        builder.Property(n => n.ExternalName)
            .HasColumnName("external_name")
            .HasMaxLength(255);

        builder.Property(n => n.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
