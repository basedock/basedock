namespace BaseDock.Domain.Entities;

public sealed class ServiceConfig
{
    public Guid ServiceId { get; private set; }

    public Guid ConfigId { get; private set; }

    public string? Target { get; private set; } // Mount path in container

    public string? Uid { get; private set; }

    public string? Gid { get; private set; }

    public string? Mode { get; private set; } // e.g., "0440"

    // Navigation properties
    public Service Service { get; private set; } = null!;

    public Config Config { get; private set; } = null!;

    private ServiceConfig()
    {
    }

    public static ServiceConfig Create(
        Guid serviceId,
        Guid configId,
        string? target,
        string? uid,
        string? gid,
        string? mode)
    {
        return new ServiceConfig
        {
            ServiceId = serviceId,
            ConfigId = configId,
            Target = target,
            Uid = uid,
            Gid = gid,
            Mode = mode
        };
    }

    public void Update(
        string? target,
        string? uid,
        string? gid,
        string? mode)
    {
        Target = target;
        Uid = uid;
        Gid = gid;
        Mode = mode;
    }
}
