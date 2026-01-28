using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class DeleteLink
{
    public record Response(string Message, string LinkId);

    public static void MapDeleteLink(this WebApplication app)
    {
        app.MapDelete("/links/{id:guid}", Handler)
           .WithName("DeleteLink")
           .WithSummary("Soft delete a link")
           .WithDescription("Deactivates a link - it will no longer redirect but analytics are preserved");
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
            return Results.BadRequest("Link is already inactive");

        link.IsActive = false;
        await db.SaveChangesAsync();

        var response = new Response(
            "Link successfully deactivated",
            link.Id.ToString()
        );

        return Results.Ok(response);
    }
}