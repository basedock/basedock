namespace BaseDock.Application.Features.Users.Commands.CreateUser;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Users.DTOs;
using BaseDock.Application.Features.Users.Mappers;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CreateUserCommandHandler(IApplicationDbContext db)
    : ICommandHandler<CreateUserCommand, Result<UserDto>>
{
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

        var passwordHash = BCrypt.Net.BCrypt.HashPassword(command.Password);

        var user = User.Create(
            command.Email,
            command.DisplayName,
            passwordHash);

        db.Users.Add(user);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(user.ToDto());
    }
}
