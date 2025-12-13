namespace BaseDock.Infrastructure.Docker;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Domain.Entities;
using BaseDock.Domain.Primitives;
using BaseDock.Infrastructure.Docker.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;

public class ComposeGeneratorService : IComposeGeneratorService
{
    private readonly IApplicationDbContext _db;
    private readonly IProjectFileService _fileService;
    private readonly ILogger<ComposeGeneratorService> _logger;

    public ComposeGeneratorService(
        IApplicationDbContext db,
        IProjectFileService fileService,
        ILogger<ComposeGeneratorService> logger)
    {
        _db = db;
        _fileService = fileService;
        _logger = logger;
    }

    public async Task<Result<string>> GenerateComposeFileAsync(
        Guid environmentId,
        string projectSlug,
        CancellationToken ct = default)
    {
        try
        {
            var environment = await _db.Environments
                .Include(e => e.Services)
                    .ThenInclude(s => s.ServiceNetworks)
                        .ThenInclude(sn => sn.Network)
                .Include(e => e.Services)
                    .ThenInclude(s => s.ServiceConfigs)
                        .ThenInclude(sc => sc.Config)
                .Include(e => e.Services)
                    .ThenInclude(s => s.ServiceSecrets)
                        .ThenInclude(ss => ss.Secret)
                .Include(e => e.Volumes)
                .Include(e => e.Networks)
                .Include(e => e.Configs)
                .Include(e => e.Secrets)
                .FirstOrDefaultAsync(e => e.Id == environmentId, ct);

            if (environment is null)
            {
                return Result.Failure<string>(Error.NotFound("Environment", environmentId.ToString()));
            }

            var compose = new ComposeFile
            {
                Services = new Dictionary<string, ComposeService>(),
                Networks = new Dictionary<string, ComposeNetwork>(),
                Volumes = new Dictionary<string, ComposeVolume>(),
                Configs = new Dictionary<string, ComposeConfig>(),
                Secrets = new Dictionary<string, ComposeSecret>()
            };

            var projectName = GetProjectName(projectSlug, environment.Slug);
            var defaultNetworkName = GetNetworkName(projectSlug, environment.Slug);

            // Add default network
            compose.Networks[defaultNetworkName] = new ComposeNetwork { Driver = "bridge" };

            // Add user-defined networks
            foreach (var network in environment.Networks)
            {
                compose.Networks[network.Name] = new ComposeNetwork
                {
                    Driver = network.Driver,
                    DriverOpts = ParseJsonDictionary(network.DriverOpts),
                    Internal = network.Internal ? true : null,
                    Attachable = network.Attachable ? true : null,
                    Labels = ParseJsonDictionary(network.Labels)
                };
            }

            // Add volumes
            foreach (var volume in environment.Volumes)
            {
                compose.Volumes[volume.Name] = new ComposeVolume
                {
                    Driver = volume.Driver,
                    DriverOpts = ParseJsonDictionary(volume.DriverOpts),
                    Labels = ParseJsonDictionary(volume.Labels),
                    External = volume.External ? true : null,
                    Name = volume.External ? volume.ExternalName : null
                };
            }

            // Add configs
            foreach (var config in environment.Configs)
            {
                if (config.External)
                {
                    compose.Configs[config.Name] = new ComposeConfig
                    {
                        External = true,
                        Name = config.ExternalName
                    };
                }
                else
                {
                    // Write config content to file
                    var configPath = Path.Combine(
                        _fileService.GetProjectPath(projectName),
                        "configs",
                        $"{config.Name}.conf");
                    Directory.CreateDirectory(Path.GetDirectoryName(configPath)!);
                    await File.WriteAllTextAsync(configPath, config.Content ?? "", ct);

                    compose.Configs[config.Name] = new ComposeConfig
                    {
                        File = configPath
                    };
                }
            }

            // Add secrets
            foreach (var secret in environment.Secrets)
            {
                if (secret.External)
                {
                    compose.Secrets[secret.Name] = new ComposeSecret
                    {
                        External = true,
                        Name = secret.ExternalName
                    };
                }
                else
                {
                    // Write secret content to file
                    var secretPath = Path.Combine(
                        _fileService.GetProjectPath(projectName),
                        "secrets",
                        $"{secret.Name}.secret");
                    Directory.CreateDirectory(Path.GetDirectoryName(secretPath)!);
                    await File.WriteAllTextAsync(secretPath, secret.Content ?? "", ct);

                    compose.Secrets[secret.Name] = new ComposeSecret
                    {
                        File = secretPath
                    };
                }
            }

            // Generate services
            foreach (var service in environment.Services)
            {
                var serviceName = GetServiceName(projectSlug, environment.Slug, service.Slug);
                var composeService = GenerateService(service, defaultNetworkName, serviceName);
                compose.Services[serviceName] = composeService;

                // Add service volumes to compose volumes
                AddVolumesFromService(compose.Volumes, composeService);
            }

            // Remove empty collections for cleaner YAML
            if (compose.Volumes?.Count == 0) compose.Volumes = null;
            if (compose.Configs?.Count == 0) compose.Configs = null;
            if (compose.Secrets?.Count == 0) compose.Secrets = null;

            // Serialize to YAML
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull)
                .Build();

            var yaml = serializer.Serialize(compose);

            // Write compose file to disk
            var writeResult = await _fileService.WriteComposeFileAsync(projectName, yaml, ct);
            if (writeResult.IsFailure)
            {
                return Result.Failure<string>(writeResult.Error);
            }

            var composePath = _fileService.GetComposeFilePath(projectName);

            _logger.LogInformation(
                "Generated compose file for environment {EnvironmentId} at {Path}",
                environmentId, composePath);

            return Result.Success(composePath);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate compose file for environment {EnvironmentId}", environmentId);
            return Result.Failure<string>(Error.Validation("ComposeGenerator.Error", ex.Message));
        }
    }

    public string GetServiceName(string projectSlug, string envSlug, string serviceSlug)
    {
        return $"{projectSlug}-{envSlug}-{serviceSlug}";
    }

    public string GetNetworkName(string projectSlug, string envSlug)
    {
        return $"{projectSlug}-{envSlug}-network";
    }

    public string GetProjectName(string projectSlug, string envSlug)
    {
        return $"{projectSlug}-{envSlug}";
    }

    private ComposeService GenerateService(Service service, string defaultNetworkName, string serviceName)
    {
        var composeService = new ComposeService
        {
            ContainerName = serviceName,
            Labels = new Dictionary<string, string>
            {
                ["basedock.service.id"] = service.Id.ToString(),
                ["basedock.service.name"] = service.Name
            }
        };

        // Image or Build
        if (!string.IsNullOrEmpty(service.Image))
        {
            composeService.Image = service.Image;
        }
        else if (!string.IsNullOrEmpty(service.BuildDockerfile))
        {
            composeService.Build = new ComposeBuild
            {
                Context = service.BuildContext ?? ".",
                Dockerfile = "Dockerfile",
                Args = ParseJsonDictionary(service.BuildArgs)
            };
        }

        // Command and entrypoint
        composeService.Command = service.Command != null ? string.Join(" ", service.Command) : null;
        composeService.Entrypoint = service.Entrypoint != null ? string.Join(" ", service.Entrypoint) : null;

        // Working dir and user
        composeService.WorkingDir = service.WorkingDir;
        composeService.User = service.User;

        // Networking
        composeService.Hostname = service.Hostname;
        composeService.Domainname = service.Domainname;

        // Networks - add default network plus any assigned networks
        composeService.Networks = new List<string> { defaultNetworkName };
        if (service.ServiceNetworks?.Any() == true)
        {
            foreach (var sn in service.ServiceNetworks)
            {
                if (sn.Network != null && !composeService.Networks.Contains(sn.Network.Name))
                {
                    composeService.Networks.Add(sn.Network.Name);
                }
            }
        }

        // Ports
        var ports = ParsePortsJson(service.Ports);
        if (ports?.Count > 0)
        {
            composeService.Ports = ports;
        }

        // Expose
        if (service.Expose?.Any() == true)
        {
            composeService.Expose = service.Expose.Select(p => p.ToString()).ToList();
        }

        // DNS
        if (service.Dns?.Any() == true)
        {
            composeService.Dns = service.Dns.ToList();
        }

        // Extra hosts
        var extraHosts = ParseJsonDictionary(service.ExtraHosts);
        if (extraHosts?.Count > 0)
        {
            composeService.ExtraHosts = extraHosts.Select(kv => $"{kv.Key}:{kv.Value}").ToList();
        }

        // Environment Variables
        var env = ParseJsonDictionary(service.EnvironmentVariables);
        if (env?.Count > 0)
        {
            composeService.Environment = env;
        }

        // Env files
        if (service.EnvFile?.Any() == true)
        {
            composeService.EnvFile = service.EnvFile.ToList();
        }

        // Volumes
        var volumes = ParseVolumesJson(service.Volumes);
        if (volumes?.Count > 0)
        {
            composeService.Volumes = volumes;
        }

        // Tmpfs
        if (service.Tmpfs?.Any() == true)
        {
            composeService.Tmpfs = service.Tmpfs.ToList();
        }

        // Dependencies
        var dependsOn = ParseDependsOnJson(service.DependsOn);
        if (dependsOn?.Count > 0)
        {
            composeService.DependsOn = dependsOn.Keys.ToList();
        }

        // Health check
        if (service.HealthcheckTest?.Any() == true && !service.HealthcheckDisabled)
        {
            composeService.Healthcheck = new ComposeHealthcheck
            {
                Test = service.HealthcheckTest.ToList(),
                Interval = service.HealthcheckInterval,
                Timeout = service.HealthcheckTimeout,
                Retries = service.HealthcheckRetries,
                StartPeriod = service.HealthcheckStartPeriod
            };
        }

        // Restart policy
        composeService.Restart = service.Restart ?? "unless-stopped";

        // Stop config
        composeService.StopGracePeriod = service.StopGracePeriod;
        composeService.StopSignal = service.StopSignal;

        // Resource limits
        if (!string.IsNullOrEmpty(service.CpuLimit) || !string.IsNullOrEmpty(service.MemoryLimit))
        {
            composeService.Deploy = new ComposeDeploy
            {
                Resources = new ComposeResources
                {
                    Limits = new ResourceSpec
                    {
                        Cpus = service.CpuLimit,
                        Memory = service.MemoryLimit
                    },
                    Reservations = !string.IsNullOrEmpty(service.CpuReservation) || !string.IsNullOrEmpty(service.MemoryReservation)
                        ? new ResourceSpec
                        {
                            Cpus = service.CpuReservation,
                            Memory = service.MemoryReservation
                        }
                        : null
                }
            };
        }

        // Replicas
        if (service.Replicas > 1)
        {
            composeService.Deploy ??= new ComposeDeploy();
            composeService.Deploy.Replicas = service.Replicas;
        }

        // Labels from service
        var customLabels = ParseJsonDictionary(service.Labels);
        if (customLabels?.Count > 0)
        {
            foreach (var label in customLabels)
            {
                composeService.Labels[label.Key] = label.Value;
            }
        }

        // Configs
        if (service.ServiceConfigs?.Any() == true)
        {
            composeService.Configs = service.ServiceConfigs.Select(sc => new ComposeConfigReference
            {
                Source = sc.Config.Name,
                Target = sc.Target,
                Uid = sc.Uid,
                Gid = sc.Gid,
                Mode = sc.Mode
            }).ToList();
        }

        // Secrets
        if (service.ServiceSecrets?.Any() == true)
        {
            composeService.Secrets = service.ServiceSecrets.Select(ss => new ComposeSecretReference
            {
                Source = ss.Secret.Name,
                Target = ss.Target,
                Uid = ss.Uid,
                Gid = ss.Gid,
                Mode = ss.Mode
            }).ToList();
        }

        return composeService;
    }

    private static void AddVolumesFromService(
        Dictionary<string, ComposeVolume>? volumes,
        ComposeService service)
    {
        if (volumes == null || service.Volumes == null) return;

        foreach (var volume in service.Volumes)
        {
            var parts = volume.Split(':');
            if (parts.Length >= 2)
            {
                var volumeName = parts[0];
                if (!volumeName.StartsWith("/") && !volumeName.StartsWith("."))
                {
                    volumes.TryAdd(volumeName, new ComposeVolume());
                }
            }
        }
    }

    private static List<string>? ParsePortsJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            var ports = System.Text.Json.JsonSerializer.Deserialize<List<PortConfig>>(json);
            return ports?.Select(p =>
            {
                var protocol = p.Protocol ?? "tcp";
                if (p.Host.HasValue)
                    return $"{p.Host}:{p.Container}/{protocol}";
                return $"{p.Container}/{protocol}";
            }).ToList();
        }
        catch
        {
            return null;
        }
    }

    private static List<string>? ParseVolumesJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            var volumes = System.Text.Json.JsonSerializer.Deserialize<List<VolumeConfig>>(json);
            return volumes?.Select(v =>
            {
                var mount = $"{v.Source}:{v.Target}";
                if (!string.IsNullOrEmpty(v.Type) && v.Type != "volume")
                {
                    mount = $"{v.Source}:{v.Target}";
                }
                return mount;
            }).ToList();
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, DependsOnCondition>? ParseDependsOnJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, DependsOnCondition>>(json);
        }
        catch
        {
            return null;
        }
    }

    private static Dictionary<string, string>? ParseJsonDictionary(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch
        {
            return null;
        }
    }

    private record PortConfig(int? Host, int Container, string? Protocol);
    private record VolumeConfig(string Source, string Target, string? Type);
    private record DependsOnCondition(string? Condition);
}
