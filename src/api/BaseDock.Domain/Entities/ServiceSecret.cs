namespace BaseDock.Domain.Entities;

public sealed class ServiceSecret
{
    public Guid ServiceId { get; private set; }

    public Guid SecretId { get; private set; }

    public string? Target { get; private set; } // Mount path (default: /run/secrets/<name>)

    public string? Uid { get; private set; }

    public string? Gid { get; private set; }

    public string? Mode { get; private set; } // e.g., "0440"

    // Navigation properties
    public Service Service { get; private set; } = null!;

    public Secret Secret { get; private set; } = null!;

    private ServiceSecret()
    {
    }

    public static ServiceSecret Create(
        Guid serviceId,
        Guid secretId,
        string? target,
        string? uid,
        string? gid,
        string? mode)
    {
        return new ServiceSecret
        {
            ServiceId = serviceId,
            SecretId = secretId,
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
