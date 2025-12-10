namespace BaseDock.Application.Features.Docker.DTOs;

public sealed record ContainerInfo(
    string Id,
    string Name,
    string Service,
    string State,
    string Status,
    IEnumerable<PortMapping> Ports);
