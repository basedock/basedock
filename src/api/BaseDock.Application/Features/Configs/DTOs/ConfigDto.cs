namespace BaseDock.Application.Features.Configs.DTOs;

public sealed record ConfigDto(
    Guid Id,
    Guid EnvironmentId,
    string Name,
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName,
    DateTimeOffset CreatedAt);

public sealed record CreateConfigRequest(
    string Name,
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName);

public sealed record UpdateConfigRequest(
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName);
