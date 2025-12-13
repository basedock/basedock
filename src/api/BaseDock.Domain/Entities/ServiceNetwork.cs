namespace BaseDock.Domain.Entities;

public sealed class ServiceNetwork
{
    public Guid ServiceId { get; private set; }

    public Guid NetworkId { get; private set; }

    public string[]? Aliases { get; private set; }

    public string? Ipv4Address { get; private set; }

    public string? Ipv6Address { get; private set; }

    // Navigation properties
    public Service Service { get; private set; } = null!;

    public Network Network { get; private set; } = null!;

    private ServiceNetwork()
    {
    }

    public static ServiceNetwork Create(
        Guid serviceId,
        Guid networkId,
        string[]? aliases,
        string? ipv4Address,
        string? ipv6Address)
    {
        return new ServiceNetwork
        {
            ServiceId = serviceId,
            NetworkId = networkId,
            Aliases = aliases,
            Ipv4Address = ipv4Address,
            Ipv6Address = ipv6Address
        };
    }

    public void Update(
        string[]? aliases,
        string? ipv4Address,
        string? ipv6Address)
    {
        Aliases = aliases;
        Ipv4Address = ipv4Address;
        Ipv6Address = ipv6Address;
    }
}
