namespace BaseDock.Application.Features.Docker.Commands.DeployProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;

public sealed record DeployProjectCommand(Guid ProjectId) : ICommand<Result<DeploymentStatusDto>>;
