namespace BaseDock.Application.Features.Secrets.Commands.UpdateSecret;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Domain.Primitives;

public sealed record UpdateSecretCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid SecretId,
    string? Content,
    string? FilePath,
    bool External,
    string? ExternalName,
    Guid UserId) : ICommand<Result<SecretDto>>;
