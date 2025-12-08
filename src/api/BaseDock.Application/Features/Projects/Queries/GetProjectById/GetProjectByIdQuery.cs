namespace BaseDock.Application.Features.Projects.Queries.GetProjectById;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetProjectByIdQuery(
    Guid ProjectId,
    Guid UserId) : IQuery<Result<ProjectDto>>;
