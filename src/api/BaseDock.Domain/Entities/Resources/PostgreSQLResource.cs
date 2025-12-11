namespace BaseDock.Domain.Entities.Resources;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class PostgreSQLResource : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Version { get; private set; } = "16";

    public int Port { get; private set; } = 5432;

    public string DatabaseName { get; private set; } = null!;

    public string Username { get; private set; } = "postgres";

    public string Password { get; private set; } = null!;

    public DeploymentStatus DeploymentStatus { get; private set; } = DeploymentStatus.NotDeployed;

    public DateTime? LastDeployedAt { get; private set; }

    public string? LastDeploymentError { get; private set; }

    public Guid EnvironmentId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    private PostgreSQLResource()
    {
    }

    public static PostgreSQLResource Create(
        string name,
        string slug,
        string? description,
        string databaseName,
        string password,
        Guid environmentId,
        string version = "16",
        int port = 5432,
        string username = "postgres")
    {
        return new PostgreSQLResource
        {
            Name = name,
            Slug = slug,
            Description = description,
            Version = version,
            Port = port,
            DatabaseName = databaseName,
            Username = username,
            Password = password,
            EnvironmentId = environmentId,
            CreatedAt = DateTime.UtcNow
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

    public void SetConfig(
        string version,
        int port,
        string databaseName,
        string username,
        string password)
    {
        Version = version;
        Port = port;
        DatabaseName = databaseName;
        Username = username;
        Password = password;
    }

    public void SetDeploymentStatus(DeploymentStatus status, DateTime? deployedAt = null)
    {
        DeploymentStatus = status;
        if (deployedAt.HasValue)
        {
            LastDeployedAt = deployedAt;
        }

        if (status != DeploymentStatus.Error)
        {
            LastDeploymentError = null;
        }
    }

    public void SetDeploymentError(string error)
    {
        DeploymentStatus = DeploymentStatus.Error;
        LastDeploymentError = error;
    }

    public string GetConnectionString()
    {
        return $"Host=localhost;Port={Port};Database={DatabaseName};Username={Username};Password={Password}";
    }
}
