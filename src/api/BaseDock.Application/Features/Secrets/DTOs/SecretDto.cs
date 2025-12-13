namespace BaseDock.Application.Features.Secrets.DTOs;

public sealed record SecretDto(
    Guid Id,
    Guid EnvironmentId,
    string Name,
    bool HasContent,
    string? FilePath,
    bool External,
    string? ExternalName,
    DateTimeOffset CreatedAt);

public sealed record CreateSecretRequest(
    string Name,
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName);

public sealed record UpdateSecretRequest(
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName);
