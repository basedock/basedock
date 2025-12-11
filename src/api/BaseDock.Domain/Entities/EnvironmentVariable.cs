namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class EnvironmentVariable : Entity
{
    public string Key { get; private set; } = null!;

    public string Value { get; private set; } = null!;

    public bool IsSecret { get; private set; }

    public Guid EnvironmentId { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    private EnvironmentVariable()
    {
    }

    public static EnvironmentVariable Create(
        string key,
        string value,
        bool isSecret,
        Guid environmentId,
        DateTimeOffset createdAt)
    {
        return new EnvironmentVariable
        {
            Key = key,
            Value = value,
            IsSecret = isSecret,
            EnvironmentId = environmentId,
            CreatedAt = createdAt
        };
    }

    public void Update(string key, string value, bool isSecret)
    {
        Key = key;
        Value = value;
        IsSecret = isSecret;
    }
}
