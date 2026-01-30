using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class BulkGetAnalytics
{
    public record Request(List<string> ShortCodes);
    
    public record Response(
        List<LinkAnalytics> Analytics,
        List<string> NotFound,
        int SuccessCount,
        int NotFoundCount
    );

    public record LinkAnalytics(
        string ShortCode,
        string OriginalUrl,
        int TotalClicks,
        DateTime CreatedAt,
        bool IsActive
    );

    public static void MapBulkGetAnalytics(this WebApplication app)
    {
        app.MapPost("/bulk/analytics", Handler)
           .WithName("BulkGetAnalytics")
           .WithSummary("Get analytics for multiple links")
           .WithDescription("Returns analytics data for multiple short URLs in a single request");
    }

    private static async Task<IResult> Handler(
        Request request,
        ShortenerDbContext db)
    {
        if (request.ShortCodes == null || !request.ShortCodes.Any())
            return Results.BadRequest("At least one short code is required");

        if (request.ShortCodes.Count > 50)
            return Results.BadRequest("Maximum 50 short codes allowed per request");

        var analytics = new List<LinkAnalytics>();
        var notFound = new List<string>();

        var links = await db.Links
            .Where(l => request.ShortCodes.Contains(l.ShortCode))
            .ToListAsync();

        var foundShortCodes = links.Select(l => l.ShortCode).ToHashSet();

        foreach (var link in links)
        {
            analytics.Add(new LinkAnalytics(
                link.ShortCode,
                link.OriginalUrl,
                link.ClickCount,
                link.CreatedAt,
                link.IsActive
            ));
        }

        foreach (var shortCode in request.ShortCodes)
        {
            if (!foundShortCodes.Contains(shortCode))
            {
                notFound.Add(shortCode);
            }
        }

        var response = new Response(
            analytics,
            notFound,
            analytics.Count,
            notFound.Count
        );

        return Results.Ok(response);
    }
}