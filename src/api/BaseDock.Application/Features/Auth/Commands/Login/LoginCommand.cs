namespace BaseDock.Application.Features.Auth.Commands.Login;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record LoginCommand(string Email, string Password) : ICommand<Result<LoginResponse>>;

public sealed record LoginResponse(Guid SessionId, DateTime ExpiresAt);

public sealed record LoginRequest(string Email, string Password);
