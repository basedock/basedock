namespace BaseDock.Application.Features.Users.Queries.GetUsers;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetUsersQuery(int? Limit = null) : IQuery<Result<IEnumerable<UserDto>>>;
