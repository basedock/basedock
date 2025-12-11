namespace BaseDock.Application.Features.Environments.Commands.CreateEnvironment;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Environments.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreateEnvironmentCommand(
    string ProjectSlug,
    string Name,
    string? Description,
    Guid UserId) : ICommand<Result<EnvironmentDto>>;
