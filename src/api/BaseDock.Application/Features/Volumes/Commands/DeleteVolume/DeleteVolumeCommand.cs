namespace BaseDock.Application.Features.Volumes.Commands.DeleteVolume;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteVolumeCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid VolumeId,
    Guid UserId) : ICommand<Result>;
