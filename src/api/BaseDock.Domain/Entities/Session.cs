namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Session : Entity
{
    public Guid UserId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset ExpiresAt { get; private set; }

    private Session()
    {
    }

    public static Session Create(Guid userId, TimeSpan expiration, DateTimeOffset createdAt)
    {
        return new Session
        {
            UserId = userId,
            CreatedAt = createdAt,
            ExpiresAt = createdAt.Add(expiration)
        };
    }

    public bool IsExpired(DateTimeOffset now) => now > ExpiresAt;
}
