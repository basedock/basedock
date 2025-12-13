namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Config : Entity
{
    public Guid EnvironmentId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Content { get; private set; }

    public string? FilePath { get; private set; }

    public bool External { get; private set; }

    public string? ExternalName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    public ICollection<ServiceConfig> ServiceConfigs { get; private set; } = new List<ServiceConfig>();

    private Config()
    {
    }

    public static Config Create(
        Guid environmentId,
        string name,
        string? content,
        string? filePath,
        bool external,
        string? externalName,
        DateTimeOffset createdAt)
    {
        return new Config
        {
            EnvironmentId = environmentId,
            Name = name,
            Content = content,
            FilePath = filePath,
            External = external,
            ExternalName = externalName,
            CreatedAt = createdAt
        };
    }

    public void Update(
        string? content,
        string? filePath,
        bool external,
        string? externalName)
    {
        Content = content;
        FilePath = filePath;
        External = external;
        ExternalName = externalName;
    }
}
