namespace BaseDock.Application.Features.Volumes.DTOs;

public sealed record VolumeDto(
    Guid Id,
    Guid EnvironmentId,
    string Name,
    string? Driver,
    string? DriverOpts,
    string? Labels,
    bool External,
    string? ExternalName,
    DateTimeOffset CreatedAt);

public sealed record CreateVolumeRequest(
    string Name,
    string? Driver,
    string? DriverOpts,
    string? Labels,
    bool External,
    string? ExternalName);
