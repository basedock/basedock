namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Project : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navigation properties
    public User CreatedBy { get; private set; } = null!;

    public ICollection<ProjectMember> Members { get; private set; } = new List<ProjectMember>();

    public ICollection<Environment> Environments { get; private set; } = new List<Environment>();

    private Project()
    {
    }

    public static Project Create(
        string name,
        string slug,
        string? description,
        Guid createdByUserId,
        DateTimeOffset createdAt)
    {
        return new Project
        {
            Name = name,
            Slug = slug,
            Description = description,
            CreatedByUserId = createdByUserId,
            CreatedAt = createdAt
        };
    }

    public void Update(string name, string? description, DateTimeOffset updatedAt)
    {
        Name = name;
        Description = description;
        UpdatedAt = updatedAt;
    }

    public void UpdateSlug(string slug, DateTimeOffset updatedAt)
    {
        Slug = slug;
        UpdatedAt = updatedAt;
    }

    public ProjectMember AddMember(Guid userId, DateTimeOffset joinedAt)
    {
        var member = ProjectMember.Create(Id, userId, joinedAt);
        Members.Add(member);
        UpdatedAt = joinedAt;
        return member;
    }

    public void RemoveMember(ProjectMember member, DateTimeOffset updatedAt)
    {
        Members.Remove(member);
        UpdatedAt = updatedAt;
    }
}
