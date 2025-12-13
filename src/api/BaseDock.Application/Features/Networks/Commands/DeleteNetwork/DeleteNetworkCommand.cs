namespace BaseDock.Application.Features.Networks.Commands.DeleteNetwork;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteNetworkCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    Guid NetworkId,
    Guid UserId) : ICommand<Result>;
