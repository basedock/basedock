namespace BaseDock.Application.Features.Resources.Commands.CreatePostgreSQLResource;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Resources.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreatePostgreSQLResourceCommand(
    string ProjectSlug,
    string EnvSlug,
    string Name,
    string? Description,
    string DatabaseName,
    string Username,
    string Password,
    string Version,
    int Port,
    Guid UserId) : ICommand<Result<PostgreSQLResourceDto>>;
