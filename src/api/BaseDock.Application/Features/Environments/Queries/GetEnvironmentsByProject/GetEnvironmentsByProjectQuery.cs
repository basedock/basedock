namespace BaseDock.Application.Features.Environments.Queries.GetEnvironmentsByProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetEnvironmentsByProjectQuery(
    string ProjectSlug,
    Guid UserId) : IQuery<Result<IEnumerable<EnvironmentDto>>>;
