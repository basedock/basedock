namespace BaseDock.Application.Features.Resources.Commands.StopResource;

using BaseDock.Application.Abstractions.Messaging;

public sealed record StopResourceCommand(
    string ProjectSlug,
    string EnvSlug,
    Guid ResourceId,
    string ResourceType,
    Guid UserId) : ICommand;
