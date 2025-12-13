namespace BaseDock.Application.Features.Services.Queries.GetServiceById;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Services.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetServiceByIdQuery(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid ServiceId,
    Guid UserId) : IQuery<Result<ServiceDetailDto>>;
