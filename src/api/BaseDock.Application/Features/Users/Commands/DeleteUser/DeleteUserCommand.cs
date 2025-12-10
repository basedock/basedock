namespace BaseDock.Application.Features.Users.Commands.DeleteUser;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteUserCommand(Guid UserId) : ICommand<Result>;
