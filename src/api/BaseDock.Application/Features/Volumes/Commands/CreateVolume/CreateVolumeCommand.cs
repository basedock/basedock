namespace BaseDock.Application.Features.Volumes.Commands.CreateVolume;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Volumes.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreateVolumeCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    string Name,
    string? Driver,
    string? DriverOpts,
    string? Labels,
    bool External,
    string? ExternalName,
    Guid UserId) : ICommand<Result<VolumeDto>>;
