namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ServiceConfigConfiguration : IEntityTypeConfiguration<ServiceConfig>
{
    public void Configure(EntityTypeBuilder<ServiceConfig> builder)
    {
        builder.ToTable("service_configs");

        builder.HasKey(sc => new { sc.ServiceId, sc.ConfigId });

        builder.Property(sc => sc.ServiceId)
            .HasColumnName("service_id");

        builder.Property(sc => sc.ConfigId)
            .HasColumnName("config_id");

        builder.Property(sc => sc.Target)
            .HasColumnName("target")
            .HasMaxLength(500);

        builder.Property(sc => sc.Uid)
            .HasColumnName("uid")
            .HasMaxLength(10);

        builder.Property(sc => sc.Gid)
            .HasColumnName("gid")
            .HasMaxLength(10);

        builder.Property(sc => sc.Mode)
            .HasColumnName("mode")
            .HasMaxLength(10);

        // Relationships
        builder.HasOne(sc => sc.Service)
            .WithMany(s => s.ServiceConfigs)
            .HasForeignKey(sc => sc.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sc => sc.Config)
            .WithMany(c => c.ServiceConfigs)
            .HasForeignKey(sc => sc.ConfigId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
