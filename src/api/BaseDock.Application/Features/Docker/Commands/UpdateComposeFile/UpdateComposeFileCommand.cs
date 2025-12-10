namespace BaseDock.Application.Features.Docker.Commands.UpdateComposeFile;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;

public sealed record UpdateComposeFileCommand(
    Guid ProjectId,
    string ComposeFileContent) : ICommand<Result<ProjectDto>>;
