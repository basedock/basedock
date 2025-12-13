namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ServiceNetworkConfiguration : IEntityTypeConfiguration<ServiceNetwork>
{
    public void Configure(EntityTypeBuilder<ServiceNetwork> builder)
    {
        builder.ToTable("service_networks");

        builder.HasKey(sn => new { sn.ServiceId, sn.NetworkId });

        builder.Property(sn => sn.ServiceId)
            .HasColumnName("service_id");

        builder.Property(sn => sn.NetworkId)
            .HasColumnName("network_id");

        builder.Property(sn => sn.Aliases)
            .HasColumnName("aliases");

        builder.Property(sn => sn.Ipv4Address)
            .HasColumnName("ipv4_address")
            .HasMaxLength(50);

        builder.Property(sn => sn.Ipv6Address)
            .HasColumnName("ipv6_address")
            .HasMaxLength(50);

        // Relationships
        builder.HasOne(sn => sn.Service)
            .WithMany(s => s.ServiceNetworks)
            .HasForeignKey(sn => sn.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sn => sn.Network)
            .WithMany(n => n.ServiceNetworks)
            .HasForeignKey(sn => sn.NetworkId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
