namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class ProjectMember : Entity
{
    public Guid ProjectId { get; private set; }

    public Guid UserId { get; private set; }

    public DateTimeOffset JoinedAt { get; private set; }

    // Navigation properties
    public Project Project { get; private set; } = null!;

    public User User { get; private set; } = null!;

    private ProjectMember()
    {
    }

    public static ProjectMember Create(Guid projectId, Guid userId, DateTimeOffset joinedAt)
    {
        return new ProjectMember
        {
            ProjectId = projectId,
            UserId = userId,
            JoinedAt = joinedAt
        };
    }
}
