namespace BaseDock.Application.Features.Users.Commands.CreateUser;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Abstractions.Security;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Application.Features.Users.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CreateUserCommandHandler(
    IApplicationDbContext db,
    IPasswordHasher passwordHasher,
    TimeProvider dateTime)
    : ICommandHandler<CreateUserCommand, Result<UserDto>>
{
    private readonly TimeProvider _dateTime = dateTime;

    public async Task<Result<UserDto>> HandleAsync(
        CreateUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var emailExists = await db.Users
            .AnyAsync(u => u.Email == command.Email.ToLowerInvariant(), cancellationToken);

        if (emailExists)
        {
            return Result.Failure<UserDto>(
                Error.Conflict("User.EmailExists", $"A user with email '{command.Email}' already exists."));
        }

        var passwordHash = passwordHasher.HashPassword(command.Password);

        var user = User.Create(
            command.Email,
            command.DisplayName,
            _dateTime.GetUtcNow(),
            passwordHash);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(user.ToDto());
    }
}
