namespace BaseDock.Application.Features.Docker.Queries.GetProjectStatus;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetProjectStatusQuery(Guid ProjectId) : IQuery<Result<DeploymentStatusDto>>;
