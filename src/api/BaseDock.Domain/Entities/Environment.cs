namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Environment : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public Guid ProjectId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public bool IsDefault { get; private set; }

    public string? ComposeFilePath { get; private set; }

    // Navigation properties
    public Project Project { get; private set; } = null!;

    public ICollection<Service> Services { get; private set; } = new List<Service>();

    public ICollection<Volume> Volumes { get; private set; } = new List<Volume>();

    public ICollection<Network> Networks { get; private set; } = new List<Network>();

    public ICollection<Config> Configs { get; private set; } = new List<Config>();

    public ICollection<Secret> Secrets { get; private set; } = new List<Secret>();

    private Environment()
    {
    }

    public static Environment Create(
        string name,
        string slug,
        string? description,
        Guid projectId,
        DateTimeOffset createdAt,
        bool isDefault = false)
    {
        return new Environment
        {
            Name = name,
            Slug = slug,
            Description = description,
            ProjectId = projectId,
            IsDefault = isDefault,
            CreatedAt = createdAt
        };
    }

    public void Update(string name, string? description)
    {
        Name = name;
        Description = description;
    }

    public void UpdateSlug(string slug)
    {
        Slug = slug;
    }

    public void SetComposeFilePath(string? path)
    {
        ComposeFilePath = path;
    }
}
