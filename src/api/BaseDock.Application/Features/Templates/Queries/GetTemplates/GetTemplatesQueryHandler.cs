namespace BaseDock.Application.Features.Templates.Queries.GetTemplates;

using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Templates.DTOs;
using BaseDock.Domain.Primitives;

public sealed class GetTemplatesQueryHandler
    : IQueryHandler<GetTemplatesQuery, Result<IEnumerable<TemplateDto>>>
{
    public Task<Result<IEnumerable<TemplateDto>>> HandleAsync(
        GetTemplatesQuery query,
        CancellationToken cancellationToken = default)
    {
        var templates = TemplateDefinitions.All.Select(t => t.ToDto());
        return Task.FromResult(Result.Success(templates));
    }
}
