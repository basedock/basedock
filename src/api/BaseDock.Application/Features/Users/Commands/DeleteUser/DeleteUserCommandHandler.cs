namespace BaseDock.Application.Features.Users.Commands.DeleteUser;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class DeleteUserCommandHandler(IApplicationDbContext db)
    : ICommandHandler<DeleteUserCommand, Result>
{
    public async Task<Result> HandleAsync(
        DeleteUserCommand command,
        CancellationToken cancellationToken = default)
    {
        var user = await db.Users
            .FirstOrDefaultAsync(u => u.Id == command.UserId, cancellationToken);

        if (user is null)
        {
            return Result.Failure(Error.NotFound("User", command.UserId));
        }

        db.Users.Remove(user);
        await db.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
