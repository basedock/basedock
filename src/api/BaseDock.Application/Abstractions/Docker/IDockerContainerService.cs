namespace BaseDock.Application.Abstractions.Docker;

using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;
using BaseDock.Domain.ValueObjects;

public interface IDockerContainerService
{
    Task<Result> RunAsync(string containerName, DockerImageConfiguration config, CancellationToken ct = default);

    Task<Result> StopAsync(string containerName, CancellationToken ct = default);

    Task<Result> StartAsync(string containerName, CancellationToken ct = default);

    Task<Result> RestartAsync(string containerName, CancellationToken ct = default);

    Task<Result> RemoveAsync(string containerName, bool force = false, CancellationToken ct = default);

    Task<Result<ContainerInfo>> GetStatusAsync(string containerName, CancellationToken ct = default);

    Task<Result<string>> GetLogsAsync(string containerName, int tailLines, CancellationToken ct = default);
}
