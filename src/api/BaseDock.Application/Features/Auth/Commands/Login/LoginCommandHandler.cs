namespace BaseDock.Application.Features.Auth.Commands.Login;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class LoginCommandHandler(IApplicationDbContext db)
    : ICommandHandler<LoginCommand, Result<LoginResponse>>
{
    private static readonly TimeSpan SessionExpiration = TimeSpan.FromDays(7);

    public async Task<Result<LoginResponse>> HandleAsync(
        LoginCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Email == command.Email.ToLowerInvariant(), cancellationToken);

        if (user is null)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        if (string.IsNullOrEmpty(user.PasswordHash))
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        var isValidPassword = BCrypt.Net.BCrypt.Verify(command.Password, user.PasswordHash);

        if (!isValidPassword)
        {
            return Result.Failure<LoginResponse>(
                Error.Unauthorized("Invalid email or password."));
        }

        var session = Session.Create(user.Id, SessionExpiration);

        db.Sessions.Add(session);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success(new LoginResponse(session.Id, session.ExpiresAt));
    }
}
