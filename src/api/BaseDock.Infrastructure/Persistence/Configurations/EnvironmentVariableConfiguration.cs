namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class EnvironmentVariableConfiguration : IEntityTypeConfiguration<EnvironmentVariable>
{
    public void Configure(EntityTypeBuilder<EnvironmentVariable> builder)
    {
        builder.ToTable("environment_variables");

        builder.HasKey(v => v.Id);

        builder.Property(v => v.Id)
            .HasColumnName("id");

        builder.Property(v => v.Key)
            .HasColumnName("key")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(v => new { v.EnvironmentId, v.Key })
            .IsUnique();

        builder.Property(v => v.Value)
            .HasColumnName("value")
            .HasColumnType("text")
            .IsRequired();

        builder.Property(v => v.IsSecret)
            .HasColumnName("is_secret")
            .HasDefaultValue(false);

        builder.Property(v => v.EnvironmentId)
            .HasColumnName("environment_id")
            .IsRequired();

        builder.Property(v => v.CreatedAt)
            .HasColumnName("created_at")
            .IsRequired();
    }
}
