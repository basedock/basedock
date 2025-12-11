namespace BaseDock.Application.Features.Environments.Queries.GetEnvironmentBySlug;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetEnvironmentBySlugQuery(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid UserId) : IQuery<Result<EnvironmentDetailDto>>;
