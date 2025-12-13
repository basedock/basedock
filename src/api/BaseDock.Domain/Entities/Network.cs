namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;

public sealed class Network : Entity
{
    public Guid EnvironmentId { get; private set; }

    public string Name { get; private set; } = null!;

    public string? Driver { get; private set; }

    public string? DriverOpts { get; private set; } // JSON

    public string? IpamDriver { get; private set; }

    public string? IpamConfig { get; private set; } // JSON array: [{"subnet": "172.28.0.0/16", "gateway": "172.28.0.1"}]

    public bool Internal { get; private set; }

    public bool Attachable { get; private set; }

    public string? Labels { get; private set; } // JSON

    public bool External { get; private set; }

    public string? ExternalName { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    // Navigation properties
    public Environment Environment { get; private set; } = null!;

    public ICollection<ServiceNetwork> ServiceNetworks { get; private set; } = new List<ServiceNetwork>();

    private Network()
    {
    }

    public static Network Create(
        Guid environmentId,
        string name,
        string? driver,
        string? driverOpts,
        string? ipamDriver,
        string? ipamConfig,
        bool @internal,
        bool attachable,
        string? labels,
        bool external,
        string? externalName,
        DateTimeOffset createdAt)
    {
        return new Network
        {
            EnvironmentId = environmentId,
            Name = name,
            Driver = driver,
            DriverOpts = driverOpts,
            IpamDriver = ipamDriver,
            IpamConfig = ipamConfig,
            Internal = @internal,
            Attachable = attachable,
            Labels = labels,
            External = external,
            ExternalName = externalName,
            CreatedAt = createdAt
        };
    }

    public void Update(
        string? driver,
        string? driverOpts,
        string? ipamDriver,
        string? ipamConfig,
        bool @internal,
        bool attachable,
        string? labels,
        bool external,
        string? externalName)
    {
        Driver = driver;
        DriverOpts = driverOpts;
        IpamDriver = ipamDriver;
        IpamConfig = ipamConfig;
        Internal = @internal;
        Attachable = attachable;
        Labels = labels;
        External = external;
        ExternalName = externalName;
    }
}
