namespace BaseDock.Application.Features.Users.Commands.CreateUser;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreateUserCommand(
    string Email,
    string DisplayName,
    string Password) : ICommand<Result<UserDto>>;
