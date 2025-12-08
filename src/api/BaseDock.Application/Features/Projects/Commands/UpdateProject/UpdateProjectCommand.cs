namespace BaseDock.Application.Features.Projects.Commands.UpdateProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record UpdateProjectCommand(
    Guid ProjectId,
    string Name,
    string? Description) : ICommand<Result<ProjectDto>>;
