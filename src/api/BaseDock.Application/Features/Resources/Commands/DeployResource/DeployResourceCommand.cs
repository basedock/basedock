namespace BaseDock.Application.Features.Resources.Commands.DeployResource;

using BaseDock.Application.Abstractions.Messaging;

public sealed record DeployResourceCommand(
    string ProjectSlug,
    string EnvSlug,
    Guid ResourceId,
    string ResourceType,
    Guid UserId) : ICommand;
