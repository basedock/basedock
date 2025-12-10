namespace BaseDock.Application.Features.Docker.DTOs;

using BaseDock.Domain.Enums;

public sealed record DeploymentStatusDto(
    DeploymentStatus Status,
    DateTime? LastDeployedAt,
    string? LastError,
    IEnumerable<ContainerInfo> Containers);
