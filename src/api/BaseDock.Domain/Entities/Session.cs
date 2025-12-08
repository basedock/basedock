namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Session : Entity
{
    public Guid UserId { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime ExpiresAt { get; private set; }

    private Session()
    {
    }

    public static Session Create(Guid userId, TimeSpan expiration)
    {
        return new Session
        {
            UserId = userId,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = DateTime.UtcNow.Add(expiration)
        };
    }

    public bool IsExpired => DateTime.UtcNow > ExpiresAt;
}
