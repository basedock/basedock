namespace BaseDock.Application.Features.Networks.Commands.CreateNetwork;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Networks.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreateNetworkCommand(
    string ProjectSlug,
    string EnvironmentSlug,
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
    Guid UserId) : ICommand<Result<NetworkDto>>;
