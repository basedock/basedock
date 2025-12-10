namespace BaseDock.Application.Features.Docker.Commands.RemoveProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Docker.DTOs;
using BaseDock.Domain.Primitives;

public sealed record RemoveProjectCommand(Guid ProjectId) : ICommand<Result<DeploymentStatusDto>>;
