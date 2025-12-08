namespace BaseDock.Application.Features.Projects.Commands.DeleteProject;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Domain.Primitives;

public sealed record DeleteProjectCommand(Guid ProjectId) : ICommand<Result>;
