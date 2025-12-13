namespace BaseDock.Domain.Entities.Resources;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class PreMadeAppResource : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public string TemplateType { get; private set; } = null!;

    public string? Configuration { get; private set; }

    public string? ServiceSlugs { get; private set; }

    public DeploymentStatus DeploymentStatus { get; private set; } = DeploymentStatus.NotDeployed;

    public DateTimeOffset? LastDeployedAt { get; private set; }

    public string? LastDeploymentError { get; private set; }

    public Guid EnvironmentId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    private PreMadeAppResource()
    {
    }

    public static PreMadeAppResource Create(
        string name,
        string slug,
        string? description,
        string templateType,
        string? configuration,
        Guid environmentId,
        DateTimeOffset createdAt)
    {
        return new PreMadeAppResource
        {
            Name = name,
            Slug = slug,
            Description = description,
            TemplateType = templateType,
            Configuration = configuration,
            EnvironmentId = environmentId,
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

    public void SetConfiguration(string? configuration)
    {
        Configuration = configuration;
    }

    public void SetServiceSlugs(string? serviceSlugs)
    {
        ServiceSlugs = serviceSlugs;
    }

    public void SetDeploymentStatus(DeploymentStatus status, DateTimeOffset? deployedAt = null)
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
