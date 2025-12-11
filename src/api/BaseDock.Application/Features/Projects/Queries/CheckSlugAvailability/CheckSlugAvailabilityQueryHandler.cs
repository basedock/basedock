namespace BaseDock.Application.Features.Projects.Queries.CheckSlugAvailability;

using BaseDock.Application.Abstractions.Data;
using BaseDock.Application.Abstractions.Messaging;
using BaseDock.Application.Features.Projects.DTOs;
using BaseDock.Domain.Primitives;
using Microsoft.EntityFrameworkCore;

public sealed class CheckSlugAvailabilityQueryHandler(IApplicationDbContext db)
    : IQueryHandler<CheckSlugAvailabilityQuery, Result<SlugAvailabilityResponse>>
{
    public async Task<Result<SlugAvailabilityResponse>> HandleAsync(
        CheckSlugAvailabilityQuery query,
        CancellationToken cancellationToken = default)
    {
        var slugExists = await db.Projects
            .AnyAsync(p => p.Slug == query.Slug, cancellationToken);

        if (!slugExists)
        {
            return Result.Success(new SlugAvailabilityResponse(true, null));
        }

        // Generate a suggested alternative slug
        var baseSlug = query.Slug;
        var suggestedSlug = baseSlug;
        var counter = 1;

        while (await db.Projects.AnyAsync(p => p.Slug == suggestedSlug, cancellationToken))
        {
            suggestedSlug = $"{baseSlug}-{counter}";
            counter++;

            // Safety limit
            if (counter > 100)
            {
                suggestedSlug = $"{baseSlug}-{Guid.NewGuid().ToString()[..8]}";
                break;
            }
        }

        return Result.Success(new SlugAvailabilityResponse(false, suggestedSlug));
    }
}
