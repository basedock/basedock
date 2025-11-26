using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using BaseDock.Application.Common.Interfaces;
using Docker.DotNet;
using Docker.DotNet.Models;

namespace BaseDock.Infrastructure.Services;

public class DockerService : IDockerService
{
    private readonly DockerClient _client;

    public DockerService()
    {
        _client = new DockerClientConfiguration(
            new Uri(RuntimeInformation.IsOSPlatform(OSPlatform.Windows) 
                ? "npipe://./pipe/docker_engine" 
                : "unix:///var/run/docker.sock"))
            .CreateClient();
    }

    public async Task<IList<ContainerListResponse>> ListContainersAsync(CancellationToken cancellationToken = default)
    {
        return await _client.Containers.ListContainersAsync(
            new ContainersListParameters { All = true }, 
            cancellationToken);
    }

    public async Task<IList<ImagesListResponse>> ListImagesAsync(CancellationToken cancellationToken = default)
    {
        return await _client.Images.ListImagesAsync(
            new ImagesListParameters { All = true }, 
            cancellationToken);
    }

    public async Task StartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _client.Containers.StartContainerAsync(containerId, new ContainerStartParameters(), cancellationToken);
    }

    public async Task StopContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _client.Containers.StopContainerAsync(containerId, new ContainerStopParameters(), cancellationToken);
    }

    public async Task RestartContainerAsync(string containerId, CancellationToken cancellationToken = default)
    {
        await _client.Containers.RestartContainerAsync(containerId, new ContainerRestartParameters(), cancellationToken);
    }
}
