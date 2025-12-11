namespace BaseDock.Application.Features.Users.Commands.UpdateUser;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Application.Features.Users.Mappers;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class UpdateUserCommandHandler(IApplicationDbContext db, TimeProvider dateTime)
    : ICommandHandler<UpdateUserCommand, Result<UserDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<UserDto>> HandleAsync(
        UpdateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == command.Id, cancellationToken);

        if (user is null)
        {
            return Result.Failure<UserDto>(Error.NotFound("User", command.Id));
        }

        if (!string.IsNullOrWhiteSpace(command.Email) &&
            command.Email.ToLowerInvariant() != user.Email)
        {
            var emailExists = await db.Users
                .AnyAsync(u => u.Email == command.Email.ToLowerInvariant() && u.Id != command.Id, cancellationToken);

            if (emailExists)
            {
                return Result.Failure<UserDto>(
                    Error.Conflict("User.EmailExists", $"A user with email '{command.Email}' already exists."));
            }
        }

        user.Update(command.DisplayName, _dateTime.GetUtcNow(), command.Email);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(user.ToDto());
    }
}
