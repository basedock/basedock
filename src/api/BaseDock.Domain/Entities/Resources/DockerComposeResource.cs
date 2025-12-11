namespace BaseDock.Domain.Entities.Resources;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class DockerComposeResource : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string ComposeFileContent { get; private set; } = null!;

    public DeploymentStatus DeploymentStatus { get; private set; } = DeploymentStatus.NotDeployed;

    public DateTime? LastDeployedAt { get; private set; }

    public string? LastDeploymentError { get; private set; }

    public Guid EnvironmentId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    private DockerComposeResource()
    {
    }

    public static DockerComposeResource Create(
        string name,
        string slug,
        string? description,
        string composeFileContent,
        Guid environmentId)
    {
        return new DockerComposeResource
        {
            Name = name,
            Slug = slug,
            Description = description,
            ComposeFileContent = composeFileContent,
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

    public void SetComposeFile(string composeFileContent)
    {
        ComposeFileContent = composeFileContent;
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
