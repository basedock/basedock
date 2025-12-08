namespace BaseDock.Application.Features.Projects.Commands.RemoveProjectMembers;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record RemoveProjectMembersCommand(
    Guid ProjectId,
    IEnumerable<Guid> UserIds) : ICommand<Result<ProjectDto>>;
