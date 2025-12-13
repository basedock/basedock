namespace BaseDock.Application.Features.Templates.Commands.ApplyTemplate;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Templates.DTOs;
using BaseDock.Domain.Primitives;

public sealed record ApplyTemplateCommand(
    string ProjectSlug,
    string EnvironmentSlug,
    string TemplateId,
    Dictionary<string, string> Parameters,
    Guid UserId) : ICommand<Result<ApplyTemplateResult>>;
