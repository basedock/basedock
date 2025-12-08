namespace BaseDock.Application.Features.Auth.Commands.Logout;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record LogoutCommand(
    string AccessToken,
    string RefreshToken) : ICommand<Result>;
