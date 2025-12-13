namespace BaseDock.Application.Features.Networks.DTOs;

public sealed record NetworkDto(
    Guid Id,
    Guid EnvironmentId,
    string Name,
    string? Driver,
    string? DriverOpts,
    string? IpamDriver,
    string? IpamConfig,
    bool Internal,
    bool Attachable,
    string? Labels,
    bool External,
    string? ExternalName,
    DateTimeOffset CreatedAt);

public sealed record CreateNetworkRequest(
    string Name,
    string? Driver,
    string? DriverOpts,
    string? IpamDriver,
    string? IpamConfig,
    bool Internal,
    bool Attachable,
    string? Labels,
    bool External,
    string? ExternalName);
