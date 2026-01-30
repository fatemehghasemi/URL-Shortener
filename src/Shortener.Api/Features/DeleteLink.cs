using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class DeleteLink
{
    public record Response(
        string Id,
        string ShortCode,
        string Message,
        DateTime DeletedAt
    );

    public static void MapDeleteLink(this WebApplication app)
    {
        app.MapDelete("/links/{id:guid}", Handler)
           .WithName("DeleteLink")
           .WithSummary("Soft delete a link")
           .WithDescription("Deactivates a link while preserving analytics data");
    }

    private static async Task<IResult> Handler(
        Guid id,
        ShortenerDbContext db)
    {
        var link = await db.Links
            .FirstOrDefaultAsync(l => l.Id == id);

        if (link == null)
            return Results.NotFound("Link not found");

        if (!link.IsActive)
            return Results.BadRequest("Link is already deleted");

        link.IsActive = false;
        await db.SaveChangesAsync();

        var response = new Response(
            link.Id.ToString(),
            link.ShortCode,
            "Link has been successfully deleted",
            DateTime.UtcNow
        );

        return Results.Ok(response);
    }
}