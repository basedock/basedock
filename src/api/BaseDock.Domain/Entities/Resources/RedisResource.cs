namespace BaseDock.Domain.Entities.Resources;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class RedisResource : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Version { get; private set; } = "7";

    public int Port { get; private set; } = 6379;

    public bool PersistenceEnabled { get; private set; } = true;

    public string? Password { get; private set; }

    public DeploymentStatus DeploymentStatus { get; private set; } = DeploymentStatus.NotDeployed;

    public DateTime? LastDeployedAt { get; private set; }

    public string? LastDeploymentError { get; private set; }

    public Guid EnvironmentId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    private RedisResource()
    {
    }

    public static RedisResource Create(
        string name,
        string slug,
        string? description,
        Guid environmentId,
        string version = "7",
        int port = 6379,
        bool persistenceEnabled = true,
        string? password = null)
    {
        return new RedisResource
        {
            Name = name,
            Slug = slug,
            Description = description,
            Version = version,
            Port = port,
            PersistenceEnabled = persistenceEnabled,
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
        bool persistenceEnabled,
        string? password)
    {
        Version = version;
        Port = port;
        PersistenceEnabled = persistenceEnabled;
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
        if (string.IsNullOrEmpty(Password))
        {
            return $"localhost:{Port}";
        }
        return $"localhost:{Port},password={Password}";
    }
}
