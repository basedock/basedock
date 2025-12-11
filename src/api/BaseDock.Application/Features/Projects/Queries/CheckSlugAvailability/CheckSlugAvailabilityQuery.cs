namespace BaseDock.Application.Features.Projects.Queries.CheckSlugAvailability;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CheckSlugAvailabilityQuery(
    string Slug) : IQuery<Result<SlugAvailabilityResponse>>;
