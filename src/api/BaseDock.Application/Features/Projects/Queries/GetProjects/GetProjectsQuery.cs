namespace BaseDock.Application.Features.Projects.Queries.GetProjects;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetProjectsQuery(
    Guid UserId,
    int? Limit = null) : IQuery<Result<IEnumerable<ProjectDto>>>;
