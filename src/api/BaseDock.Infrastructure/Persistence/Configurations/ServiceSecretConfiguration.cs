namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ServiceSecretConfiguration : IEntityTypeConfiguration<ServiceSecret>
{
    public void Configure(EntityTypeBuilder<ServiceSecret> builder)
    {
        builder.ToTable("service_secrets");

        builder.HasKey(ss => new { ss.ServiceId, ss.SecretId });

        builder.Property(ss => ss.ServiceId)
            .HasColumnName("service_id");

        builder.Property(ss => ss.SecretId)
            .HasColumnName("secret_id");

        builder.Property(ss => ss.Target)
            .HasColumnName("target")
            .HasMaxLength(500);

        builder.Property(ss => ss.Uid)
            .HasColumnName("uid")
            .HasMaxLength(10);

        builder.Property(ss => ss.Gid)
            .HasColumnName("gid")
            .HasMaxLength(10);

        builder.Property(ss => ss.Mode)
            .HasColumnName("mode")
            .HasMaxLength(10);

        // Relationships
        builder.HasOne(ss => ss.Service)
            .WithMany(s => s.ServiceSecrets)
            .HasForeignKey(ss => ss.ServiceId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(ss => ss.Secret)
            .WithMany(s => s.ServiceSecrets)
            .HasForeignKey(ss => ss.SecretId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
