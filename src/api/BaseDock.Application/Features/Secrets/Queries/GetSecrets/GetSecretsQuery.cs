namespace BaseDock.Application.Features.Secrets.Queries.GetSecrets;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetSecretsQuery(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid UserId) : IQuery<Result<IEnumerable<SecretDto>>>;
