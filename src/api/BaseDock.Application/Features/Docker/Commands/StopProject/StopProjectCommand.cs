namespace BaseDock.Application.Features.Docker.Commands.StopProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;

public sealed record StopProjectCommand(Guid ProjectId) : ICommand<Result<DeploymentStatusDto>>;
