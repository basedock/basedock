namespace BaseDock.Application.Features.Projects.DTOs;

using BaseDock.Domain.Enums;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    ProjectType ProjectType,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? ComposeFileContent,
    string? DockerImageConfig,
    DeploymentStatus DeploymentStatus,
    DateTime? LastDeployedAt,
    string? LastDeploymentError,
    IEnumerable<ProjectMemberDto> Members);

public sealed record ProjectMemberDto(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime JoinedAt);

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    ProjectType ProjectType,
    string? ComposeFileContent,
    string? DockerImageConfig,
    IEnumerable<Guid>? MemberIds);

public sealed record UpdateProjectRequest(
    string Name,
    string? Description);

public sealed record AddMembersRequest(
    IEnumerable<Guid> UserIds);

public sealed record RemoveMembersRequest(
    IEnumerable<Guid> UserIds);

public sealed record UpdateComposeFileRequest(
    string ComposeFileContent);

public sealed record CheckSlugRequest(
    string Slug);

public sealed record SlugAvailabilityResponse(
    bool IsAvailable,
    string? SuggestedSlug);
