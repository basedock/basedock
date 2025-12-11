namespace BaseDock.Application.Features.Projects.Queries.GetProjectBySlug;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetProjectBySlugQuery(
    string Slug,
    Guid UserId) : IQuery<Result<ProjectDto>>;
