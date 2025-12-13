namespace BaseDock.Infrastructure.Docker;

using System.Text.Json;
using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Abstractions.FileSystem;
using BaseDock.Domain.Entities.Resources;
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
                .Include(e => e.DockerImageResources)
                .Include(e => e.DockerfileResources)
                .Include(e => e.DockerComposeResources)
                .Include(e => e.PostgreSQLResources)
                .Include(e => e.RedisResources)
                .Include(e => e.PreMadeAppResources)
                .FirstOrDefaultAsync(e => e.Id == environmentId, ct);

            if (environment is null)
            {
                return Result.Failure<string>(Error.NotFound("Environment", environmentId.ToString()));
            }

            var compose = new ComposeFile
            {
                Services = new Dictionary<string, ComposeService>(),
                Networks = new Dictionary<string, ComposeNetwork>(),
                Volumes = new Dictionary<string, ComposeVolume>()
            };

            var networkName = GetNetworkName(projectSlug, environment.Slug);
            compose.Networks[networkName] = new ComposeNetwork { Driver = "bridge" };

            var projectName = GetProjectName(projectSlug, environment.Slug);

            // Generate services for each resource type
            foreach (var resource in environment.DockerImageResources)
            {
                var serviceName = GetServiceName(projectSlug, environment.Slug, resource.Slug);
                var service = GenerateDockerImageService(resource, networkName, serviceName);
                compose.Services[serviceName] = service;
                AddVolumesFromService(compose.Volumes, service, resource.Slug);
            }

            foreach (var resource in environment.DockerfileResources)
            {
                var serviceName = GetServiceName(projectSlug, environment.Slug, resource.Slug);
                var service = await GenerateDockerfileServiceAsync(
                    resource, projectName, networkName, serviceName, ct);
                compose.Services[serviceName] = service;
                AddVolumesFromService(compose.Volumes, service, resource.Slug);
            }

            foreach (var resource in environment.PostgreSQLResources)
            {
                var serviceName = GetServiceName(projectSlug, environment.Slug, resource.Slug);
                var service = GeneratePostgreSQLService(resource, networkName, serviceName);
                compose.Services[serviceName] = service;
                AddVolumesFromService(compose.Volumes, service, resource.Slug);
            }

            foreach (var resource in environment.RedisResources)
            {
                var serviceName = GetServiceName(projectSlug, environment.Slug, resource.Slug);
                var service = GenerateRedisService(resource, networkName, serviceName);
                compose.Services[serviceName] = service;
                AddVolumesFromService(compose.Volumes, service, resource.Slug);
            }

            foreach (var resource in environment.DockerComposeResources)
            {
                MergeDockerComposeResource(compose, resource, projectSlug, environment.Slug, networkName);
            }

            foreach (var resource in environment.PreMadeAppResources)
            {
                var services = GeneratePreMadeAppServices(resource, projectSlug, environment.Slug, networkName);
                foreach (var (name, service) in services)
                {
                    compose.Services[name] = service;
                    AddVolumesFromService(compose.Volumes, service, resource.Slug);
                }
            }

            // Remove empty collections for cleaner YAML
            if (compose.Volumes?.Count == 0) compose.Volumes = null;

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

    public string GetServiceName(string projectSlug, string envSlug, string resourceSlug)
    {
        return $"{projectSlug}-{envSlug}-{resourceSlug}";
    }

    public string GetNetworkName(string projectSlug, string envSlug)
    {
        return $"{projectSlug}-{envSlug}-network";
    }

    public string GetProjectName(string projectSlug, string envSlug)
    {
        return $"{projectSlug}-{envSlug}";
    }

    private ComposeService GenerateDockerImageService(
        DockerImageResource resource,
        string networkName,
        string serviceName)
    {
        var service = new ComposeService
        {
            Image = $"{resource.Image}:{resource.Tag}",
            ContainerName = serviceName,
            Restart = resource.RestartPolicy,
            Networks = new List<string> { networkName },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "DockerImage",
                ["basedock.resource.name"] = resource.Name
            }
        };

        ApplyCommonConfig(service, resource.Ports, resource.EnvironmentVariables,
            resource.Volumes, resource.CpuLimit, resource.MemoryLimit, resource.Slug);

        return service;
    }

    private async Task<ComposeService> GenerateDockerfileServiceAsync(
        DockerfileResource resource,
        string projectName,
        string networkName,
        string serviceName,
        CancellationToken ct)
    {
        // Write Dockerfile to filesystem
        var buildContextPath = Path.Combine(
            _fileService.GetProjectPath(projectName),
            "dockerfiles",
            resource.Slug);

        Directory.CreateDirectory(buildContextPath);
        var dockerfilePath = Path.Combine(buildContextPath, "Dockerfile");
        await File.WriteAllTextAsync(dockerfilePath, resource.DockerfileContent, ct);

        var service = new ComposeService
        {
            Build = new ComposeBuild
            {
                Context = buildContextPath,
                Dockerfile = "Dockerfile",
                Args = ParseJsonDictionary(resource.BuildArgs)
            },
            ContainerName = serviceName,
            Restart = resource.RestartPolicy,
            Networks = new List<string> { networkName },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "Dockerfile",
                ["basedock.resource.name"] = resource.Name
            }
        };

        ApplyCommonConfig(service, resource.Ports, resource.EnvironmentVariables,
            resource.Volumes, resource.CpuLimit, resource.MemoryLimit, resource.Slug);

        return service;
    }

    private ComposeService GeneratePostgreSQLService(
        PostgreSQLResource resource,
        string networkName,
        string serviceName)
    {
        return new ComposeService
        {
            Image = $"postgres:{resource.Version}",
            ContainerName = serviceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            Environment = new Dictionary<string, string>
            {
                ["POSTGRES_USER"] = resource.Username,
                ["POSTGRES_PASSWORD"] = resource.Password,
                ["POSTGRES_DB"] = resource.DatabaseName
            },
            Ports = new List<string> { $"{resource.Port}:5432" },
            Volumes = new List<string> { $"{resource.Slug}-data:/var/lib/postgresql/data" },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "PostgreSQL",
                ["basedock.resource.name"] = resource.Name
            }
        };
    }

    private ComposeService GenerateRedisService(
        RedisResource resource,
        string networkName,
        string serviceName)
    {
        var service = new ComposeService
        {
            Image = $"redis:{resource.Version}",
            ContainerName = serviceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            Ports = new List<string> { $"{resource.Port}:6379" },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "Redis",
                ["basedock.resource.name"] = resource.Name
            }
        };

        var commands = new List<string>();

        if (!string.IsNullOrEmpty(resource.Password))
        {
            commands.Add($"--requirepass {resource.Password}");
        }

        if (resource.PersistenceEnabled)
        {
            service.Volumes = new List<string> { $"{resource.Slug}-data:/data" };
            commands.Add("--appendonly yes");
        }

        if (commands.Count > 0)
        {
            service.Command = "redis-server " + string.Join(" ", commands);
        }

        return service;
    }

    private void MergeDockerComposeResource(
        ComposeFile compose,
        DockerComposeResource resource,
        string projectSlug,
        string envSlug,
        string networkName)
    {
        try
        {
            var deserializer = new DeserializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .IgnoreUnmatchedProperties()
                .Build();

            var userCompose = deserializer.Deserialize<ComposeFile>(resource.ComposeFileContent);

            if (userCompose?.Services == null) return;

            foreach (var (originalName, service) in userCompose.Services)
            {
                // Prefix service name
                var serviceName = GetServiceName(projectSlug, envSlug, $"{resource.Slug}-{originalName}");

                // Add to shared network
                service.Networks ??= new List<string>();
                if (!service.Networks.Contains(networkName))
                {
                    service.Networks.Add(networkName);
                }

                // Add labels
                service.Labels ??= new Dictionary<string, string>();
                service.Labels["basedock.resource.id"] = resource.Id.ToString();
                service.Labels["basedock.resource.type"] = "DockerCompose";
                service.Labels["basedock.resource.name"] = resource.Name;
                service.Labels["basedock.compose.original_service"] = originalName;

                // Update depends_on references
                if (service.DependsOn != null)
                {
                    service.DependsOn = service.DependsOn
                        .Select(dep => GetServiceName(projectSlug, envSlug, $"{resource.Slug}-{dep}"))
                        .ToList();
                }

                compose.Services[serviceName] = service;
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex,
                "Failed to merge docker compose resource {ResourceId}. Skipping.",
                resource.Id);
        }
    }

    private Dictionary<string, ComposeService> GeneratePreMadeAppServices(
        PreMadeAppResource resource,
        string projectSlug,
        string envSlug,
        string networkName)
    {
        var services = new Dictionary<string, ComposeService>();
        var config = ParseJsonDictionary(resource.Configuration) ?? new Dictionary<string, string>();

        switch (resource.TemplateType.ToLowerInvariant())
        {
            case "wordpress":
                GenerateWordPressServices(services, resource, projectSlug, envSlug, networkName, config);
                break;
            case "ghost":
                GenerateGhostServices(services, resource, projectSlug, envSlug, networkName, config);
                break;
            case "gitea":
                GenerateGiteaServices(services, resource, projectSlug, envSlug, networkName, config);
                break;
            default:
                _logger.LogWarning("Unknown pre-made app template type: {TemplateType}", resource.TemplateType);
                break;
        }

        return services;
    }

    private void GenerateWordPressServices(
        Dictionary<string, ComposeService> services,
        PreMadeAppResource resource,
        string projectSlug,
        string envSlug,
        string networkName,
        Dictionary<string, string> config)
    {
        var dbPassword = config.GetValueOrDefault("db_password", Guid.NewGuid().ToString("N")[..16]);
        var dbName = config.GetValueOrDefault("db_name", "wordpress");
        var dbUser = config.GetValueOrDefault("db_user", "wordpress");
        var port = config.GetValueOrDefault("port", "8080");

        var dbServiceName = GetServiceName(projectSlug, envSlug, $"{resource.Slug}-mariadb");
        var wpServiceName = GetServiceName(projectSlug, envSlug, $"{resource.Slug}-wordpress");

        services[dbServiceName] = new ComposeService
        {
            Image = "mariadb:10.11",
            ContainerName = dbServiceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            Environment = new Dictionary<string, string>
            {
                ["MYSQL_ROOT_PASSWORD"] = dbPassword,
                ["MYSQL_DATABASE"] = dbName,
                ["MYSQL_USER"] = dbUser,
                ["MYSQL_PASSWORD"] = dbPassword
            },
            Volumes = new List<string> { $"{resource.Slug}-db-data:/var/lib/mysql" },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "PreMadeApp",
                ["basedock.resource.name"] = $"{resource.Name} - Database",
                ["basedock.premade.parent"] = resource.Slug,
                ["basedock.premade.component"] = "database"
            }
        };

        services[wpServiceName] = new ComposeService
        {
            Image = config.GetValueOrDefault("wordpress_image", "wordpress:latest"),
            ContainerName = wpServiceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            DependsOn = new List<string> { dbServiceName },
            Environment = new Dictionary<string, string>
            {
                ["WORDPRESS_DB_HOST"] = dbServiceName,
                ["WORDPRESS_DB_USER"] = dbUser,
                ["WORDPRESS_DB_PASSWORD"] = dbPassword,
                ["WORDPRESS_DB_NAME"] = dbName
            },
            Ports = new List<string> { $"{port}:80" },
            Volumes = new List<string> { $"{resource.Slug}-wp-data:/var/www/html" },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "PreMadeApp",
                ["basedock.resource.name"] = $"{resource.Name} - WordPress",
                ["basedock.premade.parent"] = resource.Slug,
                ["basedock.premade.component"] = "app"
            }
        };
    }

    private void GenerateGhostServices(
        Dictionary<string, ComposeService> services,
        PreMadeAppResource resource,
        string projectSlug,
        string envSlug,
        string networkName,
        Dictionary<string, string> config)
    {
        var dbPassword = config.GetValueOrDefault("db_password", Guid.NewGuid().ToString("N")[..16]);
        var port = config.GetValueOrDefault("port", "2368");
        var url = config.GetValueOrDefault("url", $"http://localhost:{port}");

        var dbServiceName = GetServiceName(projectSlug, envSlug, $"{resource.Slug}-mysql");
        var ghostServiceName = GetServiceName(projectSlug, envSlug, $"{resource.Slug}-ghost");

        services[dbServiceName] = new ComposeService
        {
            Image = "mysql:8.0",
            ContainerName = dbServiceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            Environment = new Dictionary<string, string>
            {
                ["MYSQL_ROOT_PASSWORD"] = dbPassword,
                ["MYSQL_DATABASE"] = "ghost",
                ["MYSQL_USER"] = "ghost",
                ["MYSQL_PASSWORD"] = dbPassword
            },
            Volumes = new List<string> { $"{resource.Slug}-db-data:/var/lib/mysql" },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "PreMadeApp",
                ["basedock.resource.name"] = $"{resource.Name} - Database",
                ["basedock.premade.parent"] = resource.Slug,
                ["basedock.premade.component"] = "database"
            }
        };

        services[ghostServiceName] = new ComposeService
        {
            Image = config.GetValueOrDefault("ghost_image", "ghost:5-alpine"),
            ContainerName = ghostServiceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            DependsOn = new List<string> { dbServiceName },
            Environment = new Dictionary<string, string>
            {
                ["url"] = url,
                ["database__client"] = "mysql",
                ["database__connection__host"] = dbServiceName,
                ["database__connection__user"] = "ghost",
                ["database__connection__password"] = dbPassword,
                ["database__connection__database"] = "ghost"
            },
            Ports = new List<string> { $"{port}:2368" },
            Volumes = new List<string> { $"{resource.Slug}-content:/var/lib/ghost/content" },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "PreMadeApp",
                ["basedock.resource.name"] = $"{resource.Name} - Ghost",
                ["basedock.premade.parent"] = resource.Slug,
                ["basedock.premade.component"] = "app"
            }
        };
    }

    private void GenerateGiteaServices(
        Dictionary<string, ComposeService> services,
        PreMadeAppResource resource,
        string projectSlug,
        string envSlug,
        string networkName,
        Dictionary<string, string> config)
    {
        var dbPassword = config.GetValueOrDefault("db_password", Guid.NewGuid().ToString("N")[..16]);
        var httpPort = config.GetValueOrDefault("http_port", "3000");
        var sshPort = config.GetValueOrDefault("ssh_port", "222");

        var dbServiceName = GetServiceName(projectSlug, envSlug, $"{resource.Slug}-postgres");
        var giteaServiceName = GetServiceName(projectSlug, envSlug, $"{resource.Slug}-gitea");

        services[dbServiceName] = new ComposeService
        {
            Image = "postgres:16-alpine",
            ContainerName = dbServiceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            Environment = new Dictionary<string, string>
            {
                ["POSTGRES_USER"] = "gitea",
                ["POSTGRES_PASSWORD"] = dbPassword,
                ["POSTGRES_DB"] = "gitea"
            },
            Volumes = new List<string> { $"{resource.Slug}-db-data:/var/lib/postgresql/data" },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "PreMadeApp",
                ["basedock.resource.name"] = $"{resource.Name} - Database",
                ["basedock.premade.parent"] = resource.Slug,
                ["basedock.premade.component"] = "database"
            }
        };

        services[giteaServiceName] = new ComposeService
        {
            Image = config.GetValueOrDefault("gitea_image", "gitea/gitea:latest"),
            ContainerName = giteaServiceName,
            Restart = "unless-stopped",
            Networks = new List<string> { networkName },
            DependsOn = new List<string> { dbServiceName },
            Environment = new Dictionary<string, string>
            {
                ["USER_UID"] = "1000",
                ["USER_GID"] = "1000",
                ["GITEA__database__DB_TYPE"] = "postgres",
                ["GITEA__database__HOST"] = $"{dbServiceName}:5432",
                ["GITEA__database__NAME"] = "gitea",
                ["GITEA__database__USER"] = "gitea",
                ["GITEA__database__PASSWD"] = dbPassword
            },
            Ports = new List<string> { $"{httpPort}:3000", $"{sshPort}:22" },
            Volumes = new List<string>
            {
                $"{resource.Slug}-data:/data",
                "/etc/timezone:/etc/timezone:ro",
                "/etc/localtime:/etc/localtime:ro"
            },
            Labels = new Dictionary<string, string>
            {
                ["basedock.resource.id"] = resource.Id.ToString(),
                ["basedock.resource.type"] = "PreMadeApp",
                ["basedock.resource.name"] = $"{resource.Name} - Gitea",
                ["basedock.premade.parent"] = resource.Slug,
                ["basedock.premade.component"] = "app"
            }
        };
    }

    private void ApplyCommonConfig(
        ComposeService service,
        string? portsJson,
        string? envVarsJson,
        string? volumesJson,
        string? cpuLimit,
        string? memoryLimit,
        string resourceSlug)
    {
        // Parse ports
        var ports = ParseJsonArray(portsJson);
        if (ports?.Count > 0)
        {
            service.Ports = ports;
        }

        // Parse environment variables
        var envVars = ParseJsonDictionary(envVarsJson);
        if (envVars?.Count > 0)
        {
            service.Environment = envVars;
        }

        // Parse volumes
        var volumes = ParseJsonArray(volumesJson);
        if (volumes?.Count > 0)
        {
            service.Volumes = volumes;
        }

        // Apply resource limits
        if (!string.IsNullOrEmpty(cpuLimit) || !string.IsNullOrEmpty(memoryLimit))
        {
            service.Deploy = new ComposeDeploy
            {
                Resources = new ComposeResources
                {
                    Limits = new ResourceSpec
                    {
                        Cpus = cpuLimit,
                        Memory = memoryLimit
                    }
                }
            };
        }
    }

    private static void AddVolumesFromService(
        Dictionary<string, ComposeVolume>? volumes,
        ComposeService service,
        string resourceSlug)
    {
        if (volumes == null || service.Volumes == null) return;

        foreach (var volume in service.Volumes)
        {
            // Extract volume name (before the colon)
            var parts = volume.Split(':');
            if (parts.Length >= 2)
            {
                var volumeName = parts[0];
                // Only add named volumes (not bind mounts starting with / or .)
                if (!volumeName.StartsWith("/") && !volumeName.StartsWith("."))
                {
                    volumes.TryAdd(volumeName, new ComposeVolume());
                }
            }
        }
    }

    private static List<string>? ParseJsonArray(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return null;

        try
        {
            return JsonSerializer.Deserialize<List<string>>(json);
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
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        catch
        {
            return null;
        }
    }
}
