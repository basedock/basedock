namespace BaseDock.Application.Features.Services.DTOs;

public sealed record ServiceDto(
    Guid Id,
    Guid EnvironmentId,
    string Name,
    string Slug,
    string? Description,
    string? Image,
    string? BuildContext,
    string? BuildDockerfile,
    string DeploymentStatus,
    DateTimeOffset? LastDeployedAt,
    string? LastError,
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    string? DependsOn);

public sealed record ServiceDetailDto(
    Guid Id,
    Guid EnvironmentId,
    string Name,
    string Slug,
    string? Description,
    // Image or Build
    string? Image,
    string? BuildContext,
    string? BuildDockerfile,
    string? BuildArgs,
    // Runtime Config
    string[]? Command,
    string[]? Entrypoint,
    string? WorkingDir,
    string? User,
    // Networking
    string? Ports,
    int[]? Expose,
    string? Hostname,
    string? Domainname,
    string[]? Dns,
    string? ExtraHosts,
    // Environment Variables
    string? EnvironmentVariables,
    string[]? EnvFile,
    // Volumes & Storage
    string? Volumes,
    string[]? Tmpfs,
    // Dependencies
    string? DependsOn,
    string[]? Links,
    // Health Check
    string[]? HealthcheckTest,
    string? HealthcheckInterval,
    string? HealthcheckTimeout,
    int? HealthcheckRetries,
    string? HealthcheckStartPeriod,
    bool HealthcheckDisabled,
    // Resources
    string? CpuLimit,
    string? MemoryLimit,
    string? CpuReservation,
    string? MemoryReservation,
    // Lifecycle
    string? Restart,
    string? StopGracePeriod,
    string? StopSignal,
    // Deployment
    int Replicas,
    // Labels
    string? Labels,
    // Status
    string DeploymentStatus,
    DateTimeOffset? LastDeployedAt,
    string? LastError,
    // Metadata
    DateTimeOffset CreatedAt,
    DateTimeOffset? UpdatedAt,
    // Related
    IEnumerable<ServiceNetworkDto> Networks,
    IEnumerable<ServiceConfigDto> Configs,
    IEnumerable<ServiceSecretDto> Secrets);

public sealed record ServiceNetworkDto(
    Guid NetworkId,
    string NetworkName,
    string[]? Aliases,
    string? Ipv4Address,
    string? Ipv6Address);

public sealed record ServiceConfigDto(
    Guid ConfigId,
    string ConfigName,
    string? Target,
    string? Uid,
    string? Gid,
    string? Mode);

public sealed record ServiceSecretDto(
    Guid SecretId,
    string SecretName,
    string? Target,
    string? Uid,
    string? Gid,
    string? Mode);

public sealed record CreateServiceRequest(
    string Name,
    string? Description,
    // Image or Build
    string? Image,
    string? BuildContext,
    string? BuildDockerfile,
    string? BuildArgs,
    // Runtime Config
    string[]? Command,
    string[]? Entrypoint,
    string? WorkingDir,
    string? User,
    // Networking
    string? Ports,
    int[]? Expose,
    string? Hostname,
    string? Domainname,
    string[]? Dns,
    string? ExtraHosts,
    // Environment Variables
    string? EnvironmentVariables,
    string[]? EnvFile,
    // Volumes & Storage
    string? Volumes,
    string[]? Tmpfs,
    // Dependencies
    string? DependsOn,
    string[]? Links,
    // Health Check
    string[]? HealthcheckTest,
    string? HealthcheckInterval,
    string? HealthcheckTimeout,
    int? HealthcheckRetries,
    string? HealthcheckStartPeriod,
    bool HealthcheckDisabled,
    // Resources
    string? CpuLimit,
    string? MemoryLimit,
    string? CpuReservation,
    string? MemoryReservation,
    // Lifecycle
    string? Restart,
    string? StopGracePeriod,
    string? StopSignal,
    // Deployment
    int Replicas,
    // Labels
    string? Labels);

public sealed record UpdateServiceRequest(
    string Name,
    string? Description,
    // Image or Build
    string? Image,
    string? BuildContext,
    string? BuildDockerfile,
    string? BuildArgs,
    // Runtime Config
    string[]? Command,
    string[]? Entrypoint,
    string? WorkingDir,
    string? User,
    // Networking
    string? Ports,
    int[]? Expose,
    string? Hostname,
    string? Domainname,
    string[]? Dns,
    string? ExtraHosts,
    // Environment Variables
    string? EnvironmentVariables,
    string[]? EnvFile,
    // Volumes & Storage
    string? Volumes,
    string[]? Tmpfs,
    // Dependencies
    string? DependsOn,
    string[]? Links,
    // Health Check
    string[]? HealthcheckTest,
    string? HealthcheckInterval,
    string? HealthcheckTimeout,
    int? HealthcheckRetries,
    string? HealthcheckStartPeriod,
    bool HealthcheckDisabled,
    // Resources
    string? CpuLimit,
    string? MemoryLimit,
    string? CpuReservation,
    string? MemoryReservation,
    // Lifecycle
    string? Restart,
    string? StopGracePeriod,
    string? StopSignal,
    // Deployment
    int Replicas,
    // Labels
    string? Labels);
