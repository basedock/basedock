namespace BaseDock.Infrastructure.Persistence.Configurations;

using BaseDock.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

public class ProjectMemberConfiguration : IEntityTypeConfiguration<ProjectMember>
{
    public void Configure(EntityTypeBuilder<ProjectMember> builder)
    {
        builder.ToTable("project_members");

        builder.HasKey(pm => pm.Id);

        builder.Property(pm => pm.Id)
            .HasColumnName("id");

        builder.Property(pm => pm.ProjectId)
            .HasColumnName("project_id")
            .IsRequired();

        builder.Property(pm => pm.UserId)
            .HasColumnName("user_id")
            .IsRequired();

        builder.Property(pm => pm.JoinedAt)
            .HasColumnName("joined_at")
            .IsRequired();

        // Composite unique index to prevent duplicate memberships
        builder.HasIndex(pm => new { pm.ProjectId, pm.UserId })
            .IsUnique();

        // Relationships
        builder.HasOne(pm => pm.User)
            .WithMany()
            .HasForeignKey(pm => pm.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
