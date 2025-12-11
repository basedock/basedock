namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Project : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

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
        Guid createdByUserId)
    {
        return new Project
        {
            Name = name,
            Slug = slug,
            Description = description,
            CreatedByUserId = createdByUserId,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateSlug(string slug)
    {
        Slug = slug;
        UpdatedAt = DateTime.UtcNow;
    }

    public ProjectMember AddMember(Guid userId)
    {
        var member = ProjectMember.Create(Id, userId);
        Members.Add(member);
        UpdatedAt = DateTime.UtcNow;
        return member;
    }

    public void RemoveMember(ProjectMember member)
    {
        Members.Remove(member);
        UpdatedAt = DateTime.UtcNow;
    }
}
