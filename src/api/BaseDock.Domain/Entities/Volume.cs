namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Volume : Entity
{
    public Guid EnvironmentId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Driver { get; private set; }

    public string? DriverOpts { get; private set; } // JSON

    public string? Labels { get; private set; } // JSON

    public bool External { get; private set; }

    public string? ExternalName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    private Volume()
    {
    }

    public static Volume Create(
        Guid environmentId,
        string name,
        string? driver,
        string? driverOpts,
        string? labels,
        bool external,
        string? externalName,
        DateTimeOffset createdAt)
    {
        return new Volume
        {
            EnvironmentId = environmentId,
            Name = name,
            Driver = driver,
            DriverOpts = driverOpts,
            Labels = labels,
            External = external,
            ExternalName = externalName,
            CreatedAt = createdAt
        };
    }

    public void Update(
        string? driver,
        string? driverOpts,
        string? labels,
        bool external,
        string? externalName)
    {
        Driver = driver;
        DriverOpts = driverOpts;
        Labels = labels;
        External = external;
        ExternalName = externalName;
    }
}
