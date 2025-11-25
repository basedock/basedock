using Docker.DotNet.Models;

namespace BaseDock.Application.Common.Interfaces;

public interface IDockerService
{
    Task<IList<ContainerListResponse>> ListContainersAsync(CancellationToken cancellationToken = default);
    Task<IList<ImagesListResponse>> ListImagesAsync(CancellationToken cancellationToken = default);
    Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default);
    Task RestartContainerAsync(string containerId, CancellationToken cancellationToken = default);
}
