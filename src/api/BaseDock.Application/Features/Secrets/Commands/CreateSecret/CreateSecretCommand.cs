namespace BaseDock.Application.Features.Secrets.Commands.CreateSecret;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreateSecretCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    string Name,
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName,
    Guid UserId) : ICommand<Result<SecretDto>>;
