namespace BaseDock.Application.Features.Projects.Commands.CreateProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record CreateProjectCommand(
    string Name,
    string? Description,
    IEnumerable<Guid>? MemberIds,
    Guid CreatedByUserId) : ICommand<Result<ProjectDto>>;
