namespace BaseDock.Domain.Entities;

using BaseDock.Domain.Common;
using BaseDock.Domain.Enums;

public sealed class Service : Entity
{
    public Guid EnvironmentId { get; private set; }

    public string Name { get; private set; } = null!;

    public string Slug { get; private set; } = null!;

    public string? Description { get; private set; }

    // Image or Build
    public string? Image { get; private set; }

    public string? BuildContext { get; private set; }

    public string? BuildDockerfile { get; private set; }

    public string? BuildArgs { get; private set; } // JSON

    // Runtime Config
    public string[]? Command { get; private set; }

    public string[]? Entrypoint { get; private set; }

    public string? WorkingDir { get; private set; }

    public string? User { get; private set; }

    // Networking
    public string? Ports { get; private set; } // JSON array of {host, container, protocol}

    public int[]? Expose { get; private set; }

    public string? Hostname { get; private set; }

    public string? Domainname { get; private set; }

    public string[]? Dns { get; private set; }

    public string? ExtraHosts { get; private set; } // JSON

    // Environment Variables
    public string? EnvironmentVariables { get; private set; } // JSON

    public string[]? EnvFile { get; private set; }

    // Volumes & Storage
    public string? Volumes { get; private set; } // JSON array of {source, target, type}

    public string[]? Tmpfs { get; private set; }

    // Dependencies
    public string? DependsOn { get; private set; } // JSON {service: {condition: "..."}}

    public string[]? Links { get; private set; }

    // Health Check
    public string[]? HealthcheckTest { get; private set; }

    public string? HealthcheckInterval { get; private set; }

    public string? HealthcheckTimeout { get; private set; }

    public int? HealthcheckRetries { get; private set; }

    public string? HealthcheckStartPeriod { get; private set; }

    public bool HealthcheckDisabled { get; private set; }

    // Resources
    public string? CpuLimit { get; private set; }

    public string? MemoryLimit { get; private set; }

    public string? CpuReservation { get; private set; }

    public string? MemoryReservation { get; private set; }

    // Lifecycle
    public string? Restart { get; private set; }

    public string? StopGracePeriod { get; private set; }

    public string? StopSignal { get; private set; }

    // Deployment
    public int Replicas { get; private set; } = 1;

    // Labels
    public string? Labels { get; private set; } // JSON

    // Status
    public DeploymentStatus DeploymentStatus { get; private set; } = DeploymentStatus.NotDeployed;

    public DateTimeOffset? LastDeployedAt { get; private set; }

    public string? LastError { get; private set; }

    // Metadata
    public DateTimeOffset CreatedAt { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }

    // Navigation properties
    public Environment ParentEnvironment { get; private set; } = null!;

    public ICollection<ServiceNetwork> ServiceNetworks { get; private set; } = new List<ServiceNetwork>();

    public ICollection<ServiceConfig> ServiceConfigs { get; private set; } = new List<ServiceConfig>();

    public ICollection<ServiceSecret> ServiceSecrets { get; private set; } = new List<ServiceSecret>();

    private Service()
    {
    }

    public static Service Create(
        Guid environmentId,
        string name,
        string slug,
        string? description,
        string? image,
        DateTimeOffset createdAt)
    {
        return new Service
        {
            EnvironmentId = environmentId,
            Name = name,
            Slug = slug,
            Description = description,
            Image = image,
            CreatedAt = createdAt
        };
    }

    public void Update(
        string name,
        string? description,
        string? image,
        string? buildContext,
        string? buildDockerfile,
        string? buildArgs,
        string[]? command,
        string[]? entrypoint,
        string? workingDir,
        string? user,
        string? ports,
        int[]? expose,
        string? hostname,
        string? domainname,
        string[]? dns,
        string? extraHosts,
        string? environmentVariables,
        string[]? envFile,
        string? volumes,
        string[]? tmpfs,
        string? dependsOn,
        string[]? links,
        string[]? healthcheckTest,
        string? healthcheckInterval,
        string? healthcheckTimeout,
        int? healthcheckRetries,
        string? healthcheckStartPeriod,
        bool healthcheckDisabled,
        string? cpuLimit,
        string? memoryLimit,
        string? cpuReservation,
        string? memoryReservation,
        string? restart,
        string? stopGracePeriod,
        string? stopSignal,
        int replicas,
        string? labels,
        DateTimeOffset updatedAt)
    {
        Name = name;
        Description = description;
        Image = image;
        BuildContext = buildContext;
        BuildDockerfile = buildDockerfile;
        BuildArgs = buildArgs;
        Command = command;
        Entrypoint = entrypoint;
        WorkingDir = workingDir;
        User = user;
        Ports = ports;
        Expose = expose;
        Hostname = hostname;
        Domainname = domainname;
        Dns = dns;
        ExtraHosts = extraHosts;
        EnvironmentVariables = environmentVariables;
        EnvFile = envFile;
        Volumes = volumes;
        Tmpfs = tmpfs;
        DependsOn = dependsOn;
        Links = links;
        HealthcheckTest = healthcheckTest;
        HealthcheckInterval = healthcheckInterval;
        HealthcheckTimeout = healthcheckTimeout;
        HealthcheckRetries = healthcheckRetries;
        HealthcheckStartPeriod = healthcheckStartPeriod;
        HealthcheckDisabled = healthcheckDisabled;
        CpuLimit = cpuLimit;
        MemoryLimit = memoryLimit;
        CpuReservation = cpuReservation;
        MemoryReservation = memoryReservation;
        Restart = restart;
        StopGracePeriod = stopGracePeriod;
        StopSignal = stopSignal;
        Replicas = replicas;
        Labels = labels;
        UpdatedAt = updatedAt;
    }

    public void SetDeploymentStatus(DeploymentStatus status, DateTimeOffset? deployedAt = null, string? error = null)
    {
        DeploymentStatus = status;
        if (deployedAt.HasValue)
        {
            LastDeployedAt = deployedAt;
        }
        LastError = error;
    }
}
