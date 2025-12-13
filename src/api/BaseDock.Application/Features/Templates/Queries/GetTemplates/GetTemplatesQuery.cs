namespace BaseDock.Application.Features.Templates.Queries.GetTemplates;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Templates.DTOs;
using BaseDock.Domain.Primitives;

public sealed record GetTemplatesQuery() : IQuery<Result<IEnumerable<TemplateDto>>>;
