using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class UpdateLink
{
    public record Request(
        string? OriginalUrl,
        DateTime? ExpiresAt,
        bool? IsActive
    );

    public record Response(
        string Id,
        string ShortCode,
        string OriginalUrl,
        DateTime? ExpiresAt,
        bool IsActive,
        DateTime UpdatedAt
    );

    public static void MapUpdateLink(this WebApplication app)
    {
        app.MapPut("/links/{id:guid}", Handler)
           .WithName("UpdateLink")
           .WithSummary("Update link properties")
           .WithDescription("Update original URL, expiration date, or active status of a link");
    }

    private static async Task<IResult> Handler(
        Guid id,
        Request request,
        ShortenerDbContext db)
    {
        var link = await db.Links
            .FirstOrDefaultAsync(l => l.Id == id);

        if (link == null)
            return Results.NotFound("Link not found");

        var hasChanges = false;

        if (!string.IsNullOrWhiteSpace(request.OriginalUrl))
        {
            link.OriginalUrl = request.OriginalUrl;
            hasChanges = true;
        }

        if (request.ExpiresAt.HasValue)
        {
            if (request.ExpiresAt.Value <= DateTime.UtcNow)
                return Results.BadRequest("Expiration date must be in the future");
            
            link.ExpiresAt = request.ExpiresAt.Value;
            hasChanges = true;
        }

        if (request.IsActive.HasValue)
        {
            link.IsActive = request.IsActive.Value;
            hasChanges = true;
        }

        if (!hasChanges)
            return Results.BadRequest("No valid updates provided");

        await db.SaveChangesAsync();

        var response = new Response(
            link.Id.ToString(),
            link.ShortCode,
            link.OriginalUrl,
            link.ExpiresAt,
            link.IsActive,
            DateTime.UtcNow
        );

        return Results.Ok(response);
    }
}