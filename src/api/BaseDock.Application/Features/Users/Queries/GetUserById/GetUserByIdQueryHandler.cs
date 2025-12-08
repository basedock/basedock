namespace BaseDock.Application.Features.Users.Queries.GetUserById;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Application.Features.Users.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class GetUserByIdQueryHandler(IApplicationDbContext db)
    : IQueryHandler<GetUserByIdQuery, Result<UserDto>>
{
    public async Task<Result<UserDto>> HandleAsync(
        GetUserByIdQuery query,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == query.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("User", query.Id));
        }

        return Result.Success(user.ToDto());
    }
}
