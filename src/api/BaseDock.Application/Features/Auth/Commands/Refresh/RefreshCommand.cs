namespace BaseDock.Application.Features.Auth.Commands.Refresh;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Domain.Primitives;

public sealed record RefreshCommand(string RefreshToken) : ICommand<Result<RefreshResponse>>;

public sealed record RefreshResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string NewRefreshToken,
    UserDto User);
