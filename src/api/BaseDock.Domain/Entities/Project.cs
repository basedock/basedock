namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class Project : Entity
{
    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    public ProjectType ProjectType { get; private set; }

    public Guid CreatedByUserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    // Docker Compose configuration
    public string? ComposeFileContent { get; private set; }

    // Docker Image configuration (stored as JSON)
    public string? DockerImageConfig { get; private set; }

    public DeploymentStatus DeploymentStatus { get; private set; } = DeploymentStatus.NotDeployed;

    public DateTime? LastDeployedAt { get; private set; }

    public string? LastDeploymentError { get; private set; }

    // Navigation properties
    public User CreatedBy { get; private set; } = null!;

    public ICollection<ProjectMember> Members { get; private set; } = new List<ProjectMember>();

    private Project()
    {
    }

    public static Project Create(
        string name,
        string slug,
        string? description,
        ProjectType projectType,
        Guid createdByUserId)
    {
        return new Project
        {
            Name = name,
            Slug = slug,
            Description = description,
            ProjectType = projectType,
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

    public void SetDockerImageConfig(string? config)
    {
        DockerImageConfig = config;
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

    public void SetComposeFile(string? content)
    {
        ComposeFileContent = content;
        UpdatedAt = DateTime.UtcNow;
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

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetDeploymentError(string error)
    {
        DeploymentStatus = DeploymentStatus.Error;
        LastDeploymentError = error;
        UpdatedAt = DateTime.UtcNow;
    }
}
