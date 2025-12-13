namespace BaseDock.Api.Modules.Secrets;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Secrets.Commands.CreateSecret;
using BaseDock.Application.Features.Secrets.Commands.DeleteSecret;
using BaseDock.Application.Features.Secrets.Commands.UpdateSecret;
using BaseDock.Application.Features.Secrets.DTOs;
using BaseDock.Application.Features.Secrets.Queries.GetSecrets;
using Carter;

public class SecretsModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects/{projectSlug}/environments/{envSlug}/secrets")
            .WithTags("Secrets");

        group.MapGet("/", GetSecrets)
            .WithName("GetSecrets")
            .WithSummary("Get all secrets in an environment")
            .Produces<IEnumerable<SecretDto>>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/", CreateSecret)
            .WithName("CreateSecret")
            .WithSummary("Create a new secret")
            .Produces<SecretDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{secretId:guid}", UpdateSecret)
            .WithName("UpdateSecret")
            .WithSummary("Update a secret")
            .Produces<SecretDto>()
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{secretId:guid}", DeleteSecret)
            .WithName("DeleteSecret")
            .WithSummary("Delete a secret")
            .Produces(StatusCodes.Status204NoContent)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static async Task<IResult> GetSecrets(
        string projectSlug,
        string envSlug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetSecretsQuery(projectSlug, envSlug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateSecret(
        string projectSlug,
        string envSlug,
        CreateSecretRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new CreateSecretCommand(
            projectSlug,
            envSlug,
            request.Name,
            request.Content,
            request.FilePath,
            request.External,
            request.ExternalName,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{projectSlug}/environments/{envSlug}/secrets/{result.Value?.Id}");
    }

    private static async Task<IResult> UpdateSecret(
        string projectSlug,
        string envSlug,
        Guid secretId,
        UpdateSecretRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new UpdateSecretCommand(
            projectSlug,
            envSlug,
            secretId,
            request.Content,
            request.FilePath,
            request.External,
            request.ExternalName,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteSecret(
        string projectSlug,
        string envSlug,
        Guid secretId,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var command = new DeleteSecretCommand(projectSlug, envSlug, secretId, userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
