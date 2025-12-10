namespace BaseDock.Application.Abstractions.Docker;

using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;

public interface IDockerComposeService
{
    Task<Result> DeployAsync(string projectName, string composeFilePath, CancellationToken ct = default);

    Task<Result> StopAsync(string projectName, string composeFilePath, CancellationToken ct = default);

    Task<Result> RestartAsync(string projectName, string composeFilePath, CancellationToken ct = default);

    Task<Result> RemoveAsync(string projectName, string composeFilePath, CancellationToken ct = default);

    Task<Result<IEnumerable<ContainerInfo>>> GetStatusAsync(string projectName, CancellationToken ct = default);

    Task<Result<string>> GetLogsAsync(string projectName, string? serviceName, int tailLines, CancellationToken ct = default);
}
