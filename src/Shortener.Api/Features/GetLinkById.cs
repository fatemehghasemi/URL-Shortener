using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class GetLinkById
{
    public record Response(
        string Id,
        string ShortCode,
        string OriginalUrl,
        string ShortUrl,
        int ClickCount,
        DateTime CreatedAt,
        DateTime? ExpiresAt,
        bool IsActive,
        DateTime? LastClickAt
    );

    public static void MapGetLinkById(this WebApplication app)
    {
        app.MapGet("/links/{id:guid}", Handler)
           .WithName("GetLinkById")
           .WithSummary("Get link details by ID")
           .WithDescription("Returns detailed information about a specific link");
    }

    private static async Task<IResult> Handler(
        Guid id,
        ShortenerDbContext db,
        IConfiguration config)
    {
        var link = await db.Links
            .FirstOrDefaultAsync(l => l.Id == id);

        if (link == null)
            return Results.NotFound("Link not found");

        var lastClickAt = await db.ClickLogs
            .Where(c => c.LinkId == link.Id)
            .OrderByDescending(c => c.ClickedAt)
            .Select(c => c.ClickedAt)
            .FirstOrDefaultAsync();

        var baseUrl = config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var shortUrl = $"{baseUrl}/{link.ShortCode}";

        var response = new Response(
            link.Id.ToString(),
            link.ShortCode,
            link.OriginalUrl,
            shortUrl,
            link.ClickCount,
            link.CreatedAt,
            link.ExpiresAt,
            link.IsActive,
            lastClickAt == default ? null : lastClickAt
        );

        return Results.Ok(response);
    }
}