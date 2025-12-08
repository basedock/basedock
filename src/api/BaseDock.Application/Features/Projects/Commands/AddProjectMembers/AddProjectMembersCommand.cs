namespace BaseDock.Application.Features.Projects.Commands.AddProjectMembers;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record AddProjectMembersCommand(
    Guid ProjectId,
    IEnumerable<Guid> UserIds) : ICommand<Result<ProjectDto>>;
