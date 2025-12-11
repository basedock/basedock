namespace BaseDock.Api.Modules.Projects;

using System.Security.Claims;
using BaseDock.Api.Extensions;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.Commands.AddProjectMembers;
using BaseDock.Application.Features.Projects.Commands.CreateProject;
using BaseDock.Application.Features.Projects.Commands.DeleteProject;
using BaseDock.Application.Features.Projects.Commands.RemoveProjectMembers;
using BaseDock.Application.Features.Projects.Commands.UpdateProject;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Application.Features.Projects.Queries.CheckSlugAvailability;
using BaseDock.Application.Features.Projects.Queries.GetProjectById;
using BaseDock.Application.Features.Projects.Queries.GetProjectBySlug;
using BaseDock.Application.Features.Projects.Queries.GetProjects;
using Carter;

public class ProjectModule : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/api/projects")
            .WithTags("Projects");

        group.MapGet("/", GetProjects)
            .WithName("GetProjects")
            .WithSummary("Get all projects for the current user")
            .Produces<IEnumerable<ProjectDto>>();

        group.MapGet("/{id:guid}", GetProjectById)
            .WithName("GetProjectById")
            .WithSummary("Get project by ID")
            .Produces<ProjectDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/slug/{slug}", GetProjectBySlug)
            .WithName("GetProjectBySlug")
            .WithSummary("Get project by slug")
            .Produces<ProjectDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapGet("/check-slug", CheckSlugAvailability)
            .WithName("CheckSlugAvailability")
            .WithSummary("Check if a slug is available")
            .Produces<SlugAvailabilityResponse>();

        group.MapPost("/", CreateProject)
            .WithName("CreateProject")
            .WithSummary("Create a new project (Admin only)")
            .Produces<ProjectDto>(StatusCodes.Status201Created)
            .ProducesValidationProblem()
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPut("/{id:guid}", UpdateProject)
            .WithName("UpdateProject")
            .WithSummary("Update a project (Admin only)")
            .Produces<ProjectDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status409Conflict)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapDelete("/{id:guid}", DeleteProject)
            .WithName("DeleteProject")
            .WithSummary("Delete a project (Admin only)")
            .Produces(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/{id:guid}/members", AddMembers)
            .WithName("AddProjectMembers")
            .WithSummary("Add members to a project (Admin only)")
            .Produces<ProjectDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);

        group.MapPost("/{id:guid}/members/remove", RemoveMembers)
            .WithName("RemoveProjectMembers")
            .WithSummary("Remove members from a project (Admin only)")
            .Produces<ProjectDto>()
            .ProducesProblem(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status403Forbidden);
    }

    private static Guid GetUserId(ClaimsPrincipal user)
    {
        var userIdClaim = user.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        var isAdminClaim = user.FindFirst("isAdmin")?.Value;
        return bool.TryParse(isAdminClaim, out var isAdmin) && isAdmin;
    }

    private static async Task<IResult> GetProjects(
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        int? limit = null,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetProjectsQuery(userId, limit);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetProjectById(
        Guid id,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetProjectByIdQuery(id, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> GetProjectBySlug(
        string slug,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var userId = GetUserId(user);
        var query = new GetProjectBySlugQuery(slug, userId);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CheckSlugAvailability(
        [AsParameters] CheckSlugRequest request,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        var query = new CheckSlugAvailabilityQuery(request.Slug);
        var result = await dispatcher.QueryAsync(query, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> CreateProject(
        CreateProjectRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can create projects.");
        }

        var userId = GetUserId(user);
        var command = new CreateProjectCommand(
            request.Name,
            request.Description,
            request.ProjectType,
            request.ComposeFileContent,
            request.DockerImageConfig,
            request.MemberIds,
            userId);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToCreatedResult($"/api/projects/{result.Value?.Slug}");
    }

    private static async Task<IResult> UpdateProject(
        Guid id,
        UpdateProjectRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can update projects.");
        }

        var command = new UpdateProjectCommand(id, request.Name, request.Description);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> DeleteProject(
        Guid id,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can delete projects.");
        }

        var command = new DeleteProjectCommand(id);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> AddMembers(
        Guid id,
        AddMembersRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can manage project members.");
        }

        var command = new AddProjectMembersCommand(id, request.UserIds);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }

    private static async Task<IResult> RemoveMembers(
        Guid id,
        RemoveMembersRequest request,
        ClaimsPrincipal user,
        IDispatcher dispatcher,
        CancellationToken cancellationToken = default)
    {
        if (!IsAdmin(user))
        {
            return Results.Problem(
                statusCode: StatusCodes.Status403Forbidden,
                title: "Error.Forbidden",
                detail: "Only administrators can manage project members.");
        }

        var command = new RemoveProjectMembersCommand(id, request.UserIds);
        var result = await dispatcher.SendAsync(command, cancellationToken);
        return result.ToHttpResult();
    }
}
