namespace BaseDock.Application.Features.Users.Queries.GetUserById;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetUserByIdQuery(Guid Id) : IQuery<Result<UserDto>>;
