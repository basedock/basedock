namespace BaseDock.Application.Features.Projects.Commands.CreateProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Enums;
using BaseDock.Domain.Primitives;

public sealed record CreateProjectCommand(
    string Name,
    string Slug,
    string? Description,
    ProjectType ProjectType,
    string? ComposeFileContent,
    string? DockerImageConfig,
    IEnumerable<Guid>? MemberIds,
    Guid CreatedByUserId) : ICommand<Result<ProjectDto>>;
