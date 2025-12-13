namespace BaseDock.Application.Features.Environments.Commands.DeleteEnvironment;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteEnvironmentCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid UserId) : ICommand<Result>;
