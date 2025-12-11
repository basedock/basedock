namespace BaseDock.Domain.ValueObjects;

public sealed record DockerImageConfiguration(
    string Image,
    string? Tag,
    IEnumerable<PortMapping>? Ports,
    IDictionary<string, string>? EnvironmentVariables,
    IEnumerable<VolumeMapping>? Volumes,
    string? RestartPolicy,
    IEnumerable<string>? Networks,
    ResourceLimits? ResourceLimits);

public sealed record PortMapping(
    int ContainerPort,
    int? HostPort,
    string Protocol = "tcp");

public sealed record VolumeMapping(
    string HostPath,
    string ContainerPath,
    bool ReadOnly = false);

public sealed record ResourceLimits(
    string? CpuLimit,
    string? MemoryLimit);
