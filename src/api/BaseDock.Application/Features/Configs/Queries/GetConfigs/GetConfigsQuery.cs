namespace BaseDock.Application.Features.Configs.Queries.GetConfigs;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Configs.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetConfigsQuery(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid UserId) : IQuery<Result<IEnumerable<ConfigDto>>>;
