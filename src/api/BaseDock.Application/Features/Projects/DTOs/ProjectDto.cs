namespace BaseDock.Application.Features.Projects.DTOs;

public sealed record ProjectDto(
    Guid Id,
    string Name,
    string Slug,
    string? Description,
    Guid CreatedByUserId,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    int EnvironmentCount,
    IEnumerable<ProjectMemberDto> Members);

public sealed record ProjectMemberDto(
    Guid UserId,
    string Email,
    string DisplayName,
    DateTime JoinedAt);

public sealed record CreateProjectRequest(
    string Name,
    string? Description,
    IEnumerable<Guid>? MemberIds);

public sealed record UpdateProjectRequest(
    string Name,
    string? Description);

public sealed record AddMembersRequest(
    IEnumerable<Guid> UserIds);

public sealed record RemoveMembersRequest(
    IEnumerable<Guid> UserIds);

public sealed record CheckSlugRequest(
    string Slug);

public sealed record SlugAvailabilityResponse(
    bool IsAvailable,
    string? SuggestedSlug);
