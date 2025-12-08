namespace BaseDock.Application.Features.Users.Commands.UpdateUser;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Domain.Primitives;

public sealed record UpdateUserCommand(
    Guid Id,
    string DisplayName,
    string? Email) : ICommand<Result<UserDto>>;
