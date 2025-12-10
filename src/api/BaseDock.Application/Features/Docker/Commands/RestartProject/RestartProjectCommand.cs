namespace BaseDock.Application.Features.Docker.Commands.RestartProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;

public sealed record RestartProjectCommand(Guid ProjectId) : ICommand<Result<DeploymentStatusDto>>;
