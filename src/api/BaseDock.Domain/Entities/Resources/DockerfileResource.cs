namespace BaseDock.Domain.Entities.Resources;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class DockerfileResource : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string DockerfileContent { get; private set; } = null!;

    public string? BuildContext { get; private set; }

    // Stored as JSON dictionary
    public string? BuildArgs { get; private set; }

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

    private DockerfileResource()
    {
    }

    public static DockerfileResource Create(
        string name,
        string slug,
        string? description,
        string dockerfileContent,
        Guid environmentId)
    {
        return new DockerfileResource
        {
            Name = name,
            Slug = slug,
            Description = description,
            DockerfileContent = dockerfileContent,
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

    public void SetDockerfileConfig(
        string dockerfileContent,
        string? buildContext,
        string? buildArgs,
        string? ports,
        string? environmentVariables,
        string? volumes,
        string restartPolicy,
        string? networks,
        string? cpuLimit,
        string? memoryLimit)
    {
        DockerfileContent = dockerfileContent;
        BuildContext = buildContext;
        BuildArgs = buildArgs;
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
