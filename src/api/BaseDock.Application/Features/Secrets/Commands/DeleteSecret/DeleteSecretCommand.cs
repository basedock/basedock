namespace BaseDock.Application.Features.Secrets.Commands.DeleteSecret;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteSecretCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid SecretId,
    Guid UserId) : ICommand<Result>;
