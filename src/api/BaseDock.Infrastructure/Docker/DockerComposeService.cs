namespace BaseDock.Infrastructure.Docker;

using System.Diagnostics;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Commands;
using Ductus.FluentDocker.Extensions;
using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Logging;

public class DockerComposeService : IDockerComposeService
{
    private readonly ILogger<DockerComposeService> _logger;
    private readonly IHostService _docker;

    public DockerComposeService(ILogger<DockerComposeService> logger)
    {
        _logger = logger;
        var hosts = new Hosts().Discover();
        _docker = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.First();
    }

    public async Task<Result> DeployAsync(string projectName, string composeFilePath, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Deploying project {ProjectName} from {ComposeFilePath}", projectName, composeFilePath);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{composeFilePath}\" -p {projectName.ToLowerInvariant()} up -d --remove-orphans",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker compose process"));
            }

            await process.WaitForExitAsync(ct);

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            var error = await process.StandardError.ReadToEndAsync(ct);

            _logger.LogInformation("Docker compose output: {Output}", output);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker compose failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                return Result.Failure(Error.DockerError($"Docker compose failed: {error}"));
            }

            _logger.LogInformation("Deployment completed for project {ProjectName}", projectName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deploy project {ProjectName}", projectName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> StopAsync(string projectName, string composeFilePath, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Stopping project {ProjectName}", projectName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{composeFilePath}\" -p {projectName.ToLowerInvariant()} stop",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker compose process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker compose stop failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Docker compose stop failed: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop project {ProjectName}", projectName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> RestartAsync(string projectName, string composeFilePath, CancellationToken ct = default)
    {
        var stopResult = await StopAsync(projectName, composeFilePath, ct);
        if (stopResult.IsFailure) return stopResult;

        return await DeployAsync(projectName, composeFilePath, ct);
    }

    public async Task<Result> RemoveAsync(string projectName, string composeFilePath, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Removing project {ProjectName}", projectName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{composeFilePath}\" -p {projectName.ToLowerInvariant()} down --remove-orphans",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker compose process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker compose down failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Docker compose down failed: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove project {ProjectName}", projectName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result<IEnumerable<ContainerInfo>>> GetStatusAsync(string projectName, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var containers = _docker.GetContainers(true);
                var projectNameLower = projectName.ToLowerInvariant();

                var projectContainers = containers
                    .Where(c =>
                    {
                        var config = c.GetConfiguration();
                        return config?.Config?.Labels?.TryGetValue("com.docker.compose.project", out var project) == true
                            && project == projectNameLower;
                    })
                    .Select(MapToContainerInfo)
                    .ToList();

                _logger.LogInformation("Found {ContainerCount} containers for project {ProjectName}", projectContainers.Count, projectName);

                return Result.Success<IEnumerable<ContainerInfo>>(projectContainers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get status for project {ProjectName}", projectName);
                return Result.Failure<IEnumerable<ContainerInfo>>(Error.DockerConnectionFailed(ex.Message));
            }
        }, ct);
    }

    public async Task<Result<string>> GetLogsAsync(
        string projectName,
        string? serviceName,
        int tailLines,
        CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var statusResult = GetStatusAsync(projectName, ct).GetAwaiter().GetResult();
                if (statusResult.IsFailure)
                    return Result.Failure<string>(statusResult.Error);

                var containers = statusResult.Value.ToList();
                if (containers.Count == 0)
                    return Result.Success("No containers found for this project.");

                if (!string.IsNullOrEmpty(serviceName))
                {
                    containers = containers
                        .Where(c => c.Service.Equals(serviceName, StringComparison.OrdinalIgnoreCase))
                        .ToList();

                    if (containers.Count == 0)
                        return Result.Failure<string>(Error.NotFound("Service", serviceName));
                }

                var allLogs = new List<string>();

                foreach (var containerInfo in containers)
                {
                    try
                    {
                        var container = _docker.GetContainers()
                            .FirstOrDefault(c => c.Id.StartsWith(containerInfo.Id));

                        if (container != null)
                        {
                            using var logs = _docker.Host.Logs(
                                container.Id,
                                showTimeStamps: true,
                                numLines: tailLines,
                                certificates: _docker.Certificates);
                            allLogs.Add($"=== {containerInfo.Service} ({containerInfo.Name}) ===");
                            allLogs.AddRange(logs.ReadToEnd());
                            allLogs.Add(string.Empty);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to get logs for container {ContainerId}", containerInfo.Id);
                        allLogs.Add($"=== {containerInfo.Service} ({containerInfo.Name}) ===");
                        allLogs.Add($"Error getting logs: {ex.Message}");
                        allLogs.Add(string.Empty);
                    }
                }

                return Result.Success(string.Join("\n", allLogs));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get logs for project {ProjectName}", projectName);
                return Result.Failure<string>(Error.DockerError($"Failed to get logs: {ex.Message}"));
            }
        }, ct);
    }

    public async Task<Result> DeployServiceAsync(
        string projectName,
        string composeFilePath,
        string serviceName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Deploying service {ServiceName} in project {ProjectName}",
                serviceName, projectName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{composeFilePath}\" -p {projectName.ToLowerInvariant()} up -d {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker compose process"));
            }

            await process.WaitForExitAsync(ct);

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            var error = await process.StandardError.ReadToEndAsync(ct);

            _logger.LogInformation("Docker compose service deploy output: {Output}", output);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker compose service deploy failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Failed to deploy service: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to deploy service {ServiceName}", serviceName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> StopServiceAsync(
        string projectName,
        string composeFilePath,
        string serviceName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Stopping service {ServiceName} in project {ProjectName}",
                serviceName, projectName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{composeFilePath}\" -p {projectName.ToLowerInvariant()} stop {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker compose process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker compose service stop failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Failed to stop service: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop service {ServiceName}", serviceName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> RestartServiceAsync(
        string projectName,
        string composeFilePath,
        string serviceName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Restarting service {ServiceName} in project {ProjectName}",
                serviceName, projectName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{composeFilePath}\" -p {projectName.ToLowerInvariant()} restart {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker compose process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker compose service restart failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Failed to restart service: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart service {ServiceName}", serviceName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> StartServiceAsync(
        string projectName,
        string composeFilePath,
        string serviceName,
        CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation(
                "Starting service {ServiceName} in project {ProjectName}",
                serviceName, projectName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"compose -f \"{composeFilePath}\" -p {projectName.ToLowerInvariant()} start {serviceName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker compose process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker compose service start failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Failed to start service: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start service {ServiceName}", serviceName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    private static ContainerInfo MapToContainerInfo(IContainerService container)
    {
        var config = container.GetConfiguration();

        var serviceName = config?.Config?.Labels?.TryGetValue("com.docker.compose.service", out var svc) == true
            ? svc
            : "unknown";

        var ports = new List<PortMapping>();
        if (config?.NetworkSettings?.Ports != null)
        {
            foreach (var port in config.NetworkSettings.Ports)
            {
                if (port.Value == null) continue;

                var portParts = port.Key.Split('/');
                var privatePort = int.Parse(portParts[0]);
                var protocol = portParts.Length > 1 ? portParts[1] : "tcp";

                foreach (var binding in port.Value)
                {
                    ports.Add(new PortMapping(
                        privatePort,
                        binding.HostPort != null ? int.Parse(binding.HostPort) : null,
                        protocol));
                }
            }
        }

        var name = container.Name ?? container.Id[..Math.Min(12, container.Id.Length)];
        var id = container.Id[..Math.Min(12, container.Id.Length)];

        return new ContainerInfo(
            id,
            name,
            serviceName,
            config?.State?.Status ?? "unknown",
            config?.State?.Status ?? "unknown",
            ports);
    }
}
