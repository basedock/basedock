namespace BaseDock.Application.Features.Services.Queries.GetServices;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetServicesQuery(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid UserId) : IQuery<Result<IEnumerable<ServiceDto>>>;
