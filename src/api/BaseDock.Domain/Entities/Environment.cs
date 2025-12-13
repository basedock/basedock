namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;
using BaseDock.Domain.Entities.Resources;

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

    public ICollection<EnvironmentVariable> Variables { get; private set; } = new List<EnvironmentVariable>();

    public ICollection<DockerImageResource> DockerImageResources { get; private set; } = new List<DockerImageResource>();

    public ICollection<DockerfileResource> DockerfileResources { get; private set; } = new List<DockerfileResource>();

    public ICollection<DockerComposeResource> DockerComposeResources { get; private set; } = new List<DockerComposeResource>();

    public ICollection<PostgreSQLResource> PostgreSQLResources { get; private set; } = new List<PostgreSQLResource>();

    public ICollection<RedisResource> RedisResources { get; private set; } = new List<RedisResource>();

    public ICollection<PreMadeAppResource> PreMadeAppResources { get; private set; } = new List<PreMadeAppResource>();

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
