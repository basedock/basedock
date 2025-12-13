namespace BaseDock.Api.Modules.Templates;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Templates.Commands.ApplyTemplate;
using BaseDock.Application.Features.Templates.DTOs;
using BaseDock.Application.Features.Templates.Queries.GetTemplates;
using Carter;

public class TemplatesModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // Global templates endpoint
        app.MapGet("/api/templates", GetTemplates)
            .WithTags("Templates")
            .WithName("GetTemplates")
            .WithSummary("Get all available templates")
            .Produces<IEnumerable<TemplateDto>>();

        // Apply template to environment
        app.MapPost("/api/projects/{projectSlug}/environments/{envSlug}/templates/{templateId}/apply", ApplyTemplate)
            .WithTags("Templates")
            .WithName("ApplyTemplate")
            .WithSummary("Apply a template to create services in an environment")
            .Produces<ApplyTemplateResult>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static async Task<IResult> GetTemplates(
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var query = new GetTemplatesQuery();
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> ApplyTemplate(
        string projectSlug,
        string envSlug,
        string templateId,
        ApplyTemplateRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new ApplyTemplateCommand(
            projectSlug,
            envSlug,
            templateId,
            request.Parameters,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{envSlug}/services");
    }
}
