namespace BaseDock.Infrastructure.Docker;

using System.Diagnostics;
using System.Text;
using BaseDock.Application.Abstractions.Docker;
using BaseDock.Domain.Primitives;
using BaseDock.Domain.ValueObjects;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Services;
using Microsoft.Extensions.Logging;
using ContainerInfo = BaseDock.Application.Features.Docker.DTOs.ContainerInfo;
using PortMapping = BaseDock.Application.Features.Docker.DTOs.PortMapping;

public class DockerContainerService : IDockerContainerService
{
    private readonly ILogger<DockerContainerService> _logger;
    private readonly IHostService _docker;
    private const string ProjectLabelKey = "basedock.project";

    public DockerContainerService(ILogger<DockerContainerService> logger)
    {
        _logger = logger;
        var hosts = new Hosts().Discover();
        _docker = hosts.FirstOrDefault(x => x.IsNative) ?? hosts.First();
    }

    public async Task<Result> RunAsync(string containerName, DockerImageConfiguration config, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Running container {ContainerName} with image {Image}:{Tag}",
                containerName, config.Image, config.Tag ?? "latest");

            var args = BuildDockerRunCommand(containerName, config);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = args,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker run process"));
            }

            await process.WaitForExitAsync(ct);

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            var error = await process.StandardError.ReadToEndAsync(ct);

            _logger.LogInformation("Docker run output: {Output}", output);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker run failed with exit code {ExitCode}: {Error}", process.ExitCode, error);
                return Result.Failure(Error.DockerError($"Docker run failed: {error}"));
            }

            _logger.LogInformation("Container {ContainerName} started successfully", containerName);
            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to run container {ContainerName}", containerName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> StopAsync(string containerName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Stopping container {ContainerName}", containerName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"stop {containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker stop process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker stop failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Docker stop failed: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to stop container {ContainerName}", containerName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> StartAsync(string containerName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Starting container {ContainerName}", containerName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"start {containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker start process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker start failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Docker start failed: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to start container {ContainerName}", containerName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> RestartAsync(string containerName, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Restarting container {ContainerName}", containerName);

            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"restart {containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker restart process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker restart failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Docker restart failed: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart container {ContainerName}", containerName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result> RemoveAsync(string containerName, bool force = false, CancellationToken ct = default)
    {
        try
        {
            _logger.LogInformation("Removing container {ContainerName} (force: {Force})", containerName, force);

            var forceFlag = force ? "-f " : "";
            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"rm {forceFlag}{containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure(Error.DockerError("Failed to start docker rm process"));
            }

            await process.WaitForExitAsync(ct);

            var error = await process.StandardError.ReadToEndAsync(ct);

            // Exit code 1 with "No such container" is acceptable when removing
            if (process.ExitCode != 0 && !error.Contains("No such container"))
            {
                _logger.LogError("Docker rm failed: {Error}", error);
                return Result.Failure(Error.DockerError($"Docker rm failed: {error}"));
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to remove container {ContainerName}", containerName);
            return Result.Failure(Error.DockerError(ex.Message));
        }
    }

    public async Task<Result<ContainerInfo>> GetStatusAsync(string containerName, CancellationToken ct = default)
    {
        return await Task.Run(() =>
        {
            try
            {
                var containers = _docker.GetContainers(true);
                var container = containers.FirstOrDefault(c =>
                    c.Name?.Equals(containerName, StringComparison.OrdinalIgnoreCase) == true ||
                    c.Name?.Equals($"/{containerName}", StringComparison.OrdinalIgnoreCase) == true);

                if (container == null)
                {
                    return Result.Failure<ContainerInfo>(
                        Error.NotFound("Container", containerName));
                }

                var containerInfo = MapToContainerInfo(container, containerName);
                return Result.Success(containerInfo);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get status for container {ContainerName}", containerName);
                return Result.Failure<ContainerInfo>(Error.DockerConnectionFailed(ex.Message));
            }
        }, ct);
    }

    public async Task<Result<string>> GetLogsAsync(string containerName, int tailLines, CancellationToken ct = default)
    {
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"logs --tail {tailLines} --timestamps {containerName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return Result.Failure<string>(Error.DockerError("Failed to start docker logs process"));
            }

            await process.WaitForExitAsync(ct);

            var output = await process.StandardOutput.ReadToEndAsync(ct);
            var error = await process.StandardError.ReadToEndAsync(ct);

            // Docker logs outputs to stderr for some messages, combine both
            var allLogs = new StringBuilder();
            allLogs.AppendLine($"=== {containerName} ===");

            if (!string.IsNullOrEmpty(output))
            {
                allLogs.Append(output);
            }
            if (!string.IsNullOrEmpty(error) && process.ExitCode == 0)
            {
                // stderr might contain actual logs in some cases
                allLogs.Append(error);
            }

            if (process.ExitCode != 0)
            {
                if (error.Contains("No such container"))
                {
                    return Result.Success("No container found.");
                }
                return Result.Failure<string>(Error.DockerError($"Failed to get logs: {error}"));
            }

            return Result.Success(allLogs.ToString());
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get logs for container {ContainerName}", containerName);
            return Result.Failure<string>(Error.DockerError($"Failed to get logs: {ex.Message}"));
        }
    }

    private string BuildDockerRunCommand(string containerName, DockerImageConfiguration config)
    {
        var args = new StringBuilder();
        args.Append("run -d ");
        args.Append($"--name {containerName} ");
        args.Append($"-l {ProjectLabelKey}={containerName} ");

        // Ports
        if (config.Ports != null)
        {
            foreach (var port in config.Ports)
            {
                if (port.HostPort.HasValue)
                {
                    args.Append($"-p {port.HostPort}:{port.ContainerPort}/{port.Protocol} ");
                }
                else
                {
                    args.Append($"-p {port.ContainerPort}/{port.Protocol} ");
                }
            }
        }

        // Environment variables
        if (config.EnvironmentVariables != null)
        {
            foreach (var (key, value) in config.EnvironmentVariables)
            {
                // Escape double quotes in values
                var escapedValue = value.Replace("\"", "\\\"");
                args.Append($"-e \"{key}={escapedValue}\" ");
            }
        }

        // Volumes
        if (config.Volumes != null)
        {
            foreach (var vol in config.Volumes)
            {
                var ro = vol.ReadOnly ? ":ro" : "";
                args.Append($"-v \"{vol.HostPath}:{vol.ContainerPath}{ro}\" ");
            }
        }

        // Restart policy
        if (!string.IsNullOrEmpty(config.RestartPolicy))
        {
            args.Append($"--restart={config.RestartPolicy} ");
        }

        // Networks
        if (config.Networks != null)
        {
            foreach (var network in config.Networks)
            {
                args.Append($"--network={network} ");
            }
        }

        // Resource limits
        if (config.ResourceLimits != null)
        {
            if (!string.IsNullOrEmpty(config.ResourceLimits.CpuLimit))
            {
                args.Append($"--cpus={config.ResourceLimits.CpuLimit} ");
            }
            if (!string.IsNullOrEmpty(config.ResourceLimits.MemoryLimit))
            {
                args.Append($"-m {config.ResourceLimits.MemoryLimit} ");
            }
        }

        // Image
        var tag = config.Tag ?? "latest";
        args.Append($"{config.Image}:{tag}");

        return args.ToString();
    }

    private static ContainerInfo MapToContainerInfo(IContainerService container, string serviceName)
    {
        var config = container.GetConfiguration();

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
        // Remove leading slash from container name if present
        if (name.StartsWith('/'))
        {
            name = name[1..];
        }
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
