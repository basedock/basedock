namespace BaseDock.Application.Features.Configs.Commands.DeleteConfig;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteConfigCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid ConfigId,
    Guid UserId) : ICommand<Result>;
