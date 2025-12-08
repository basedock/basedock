namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class User : Entity
{
    public string Email { get; private set; } = null!;

    public string DisplayName { get; private set; } = null!;

    public string? PasswordHash { get; private set; }

    public bool IsAdmin { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    private User()
    {
    }

    public static User Create(string email, string displayName, string? passwordHash = null, bool isAdmin = false)
    {
        return new User
        {
            Email = email.ToLowerInvariant(),
            DisplayName = displayName,
            PasswordHash = passwordHash,
            IsAdmin = isAdmin,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void Update(string displayName, string? email = null)
    {
        DisplayName = displayName;

        if (!string.IsNullOrWhiteSpace(email))
        {
            Email = email.ToLowerInvariant();
        }

        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPasswordHash(string passwordHash)
    {
        PasswordHash = passwordHash;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAdmin(bool isAdmin)
    {
        IsAdmin = isAdmin;
        UpdatedAt = DateTime.UtcNow;
    }
}
