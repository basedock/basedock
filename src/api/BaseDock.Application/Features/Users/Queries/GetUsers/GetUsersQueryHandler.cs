namespace BaseDock.Application.Features.Users.Queries.GetUsers;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Application.Features.Users.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetUsersQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetUsersQuery, Result<IEnumerable<UserDto>>>
{
    public async Task<Result<IEnumerable<UserDto>>> HandleAsync(
        GetUsersQuery query,
        CancellationToken cancellationToken = default)
    {
        var usersQuery = db.Users
            .AsNoTracking()
            .OrderByDescending(u => u.CreatedAt);

        var users = query.Limit.HasValue
            ? await usersQuery.Take(query.Limit.Value).ToListAsync(cancellationToken)
            : await usersQuery.ToListAsync(cancellationToken);

        return Result.Success(users.ToDtos());
    }
}
