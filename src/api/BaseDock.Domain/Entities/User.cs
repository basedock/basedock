namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class User : Entity
{
    public string Email { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public string? PasswordHash { get; private set; }

    public bool IsAdmin { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    private User()
    {
    }

    public static User Create(string email, string displayName, DateTimeOffset createdAt, string? passwordHash = null, bool isAdmin = false)
    {
        return new User
        {
            Email = email.ToLowerInvariant(),
            DisplayName = displayName,
            PasswordHash = passwordHash,
            IsAdmin = isAdmin,
            CreatedAt = createdAt
        };
    }

    public void Update(string displayName, DateTimeOffset updatedAt, string? email = null)
    {
        DisplayName = displayName;

        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email.ToLowerInvariant();
        }

        UpdatedAt = updatedAt;
    }

    public void SetPasswordHash(string passwordHash, DateTimeOffset updatedAt)
    {
        PasswordHash = passwordHash;
        UpdatedAt = updatedAt;
    }

    public void SetAdmin(bool isAdmin, DateTimeOffset updatedAt)
    {
        IsAdmin = isAdmin;
        UpdatedAt = updatedAt;
    }
}
