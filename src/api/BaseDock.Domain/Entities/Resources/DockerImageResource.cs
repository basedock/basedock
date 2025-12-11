namespace BaseDock.Domain.Entities.Resources;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class DockerImageResource : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string Image { get; private set; } = null!;

    public string Tag { get; private set; } = "latest";

    // Stored as JSON
    public string? Ports { get; private set; }

    // Stored as JSON dictionary
    public string? EnvironmentVariables { get; private set; }

    // Stored as JSON
    public string? Volumes { get; private set; }

    public string RestartPolicy { get; private set; } = "unless-stopped";

    // Stored as JSON array
    public string? Networks { get; private set; }

    public string? CpuLimit { get; private set; }

    public string? MemoryLimit { get; private set; }

    public DeploymentStatus DeploymentStatus { get; private set; } = DeploymentStatus.NotDeployed;

    public DateTime? LastDeployedAt { get; private set; }

    public string? LastDeploymentError { get; private set; }

    public Guid EnvironmentId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    private DockerImageResource()
    {
    }

    public static DockerImageResource Create(
        string name,
        string slug,
        string? description,
        string image,
        string tag,
        Guid environmentId)
    {
        return new DockerImageResource
        {
            Name = name,
            Slug = slug,
            Description = description,
            Image = image,
            Tag = tag,
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

    public void SetImageConfig(
        string image,
        string tag,
        string? ports,
        string? environmentVariables,
        string? volumes,
        string restartPolicy,
        string? networks,
        string? cpuLimit,
        string? memoryLimit)
    {
        Image = image;
        Tag = tag;
        Ports = ports;
        EnvironmentVariables = environmentVariables;
        Volumes = volumes;
        RestartPolicy = restartPolicy;
        Networks = networks;
        CpuLimit = cpuLimit;
        MemoryLimit = memoryLimit;
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
}
