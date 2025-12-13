namespace BaseDock.Application.Features.Networks.Queries.GetNetworks;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Networks.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetNetworksQuery(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid UserId) : IQuery<Result<IEnumerable<NetworkDto>>>;
