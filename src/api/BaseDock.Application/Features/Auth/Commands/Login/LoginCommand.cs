namespace BaseDock.Application.Features.Auth.Commands.Login;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Domain.Primitives;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>;

public sealed record LoginResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    UserDto User);

public sealed record LoginRequest(string Email, string Password);
