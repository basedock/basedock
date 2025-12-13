namespace BaseDock.Application.Features.Volumes.Queries.GetVolumes;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Volumes.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetVolumesQuery(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid UserId) : IQuery<Result<IEnumerable<VolumeDto>>>;
