namespace BaseDock.Application.Features.Docker.DTOs;

public sealed record PortMapping(
    int PrivatePort,
    int? PublicPort,
    string Protocol);
