using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class GetLinkAnalytics
{
    public record Response(
        string LinkId,
        string ShortCode,
        string OriginalUrl,
        int TotalClicks,
        DateTime CreatedAt,
        DateTime? LastClickAt,
        List<ClickData> RecentClicks,
        List<ReferrerData> TopReferrers,
        List<HourlyClickData> HourlyStats
    );

    public record ClickData(
        DateTime ClickedAt,
        string? IpAddress,
        string? UserAgent,
        string? Referer
    );

    public record ReferrerData(
        string Referer,
        int Count
    );

    public record HourlyClickData(
        DateTime Hour,
        int Clicks
    );

    public static void MapGetLinkAnalytics(this WebApplication app)
    {
        app.MapGet("/analytics/{shortCode}", Handler)
           .WithName("GetLinkAnalytics")
           .WithSummary("Get analytics for a short URL")
           .WithDescription("Returns detailed analytics including clicks, referrers, and timeline data");
    }

    private static async Task<IResult> Handler(
        string shortCode,
        ShortenerDbContext db)
    {
        var link = await db.Links
            .Include(l => l.ClickLogs)
            .FirstOrDefaultAsync(l => l.ShortCode == shortCode && l.IsActive);

        if (link == null)
            return Results.NotFound("Short URL not found");

        var clickLogs = link.ClickLogs.OrderByDescending(c => c.ClickedAt).ToList();

        var recentClicks = clickLogs
            .Take(50)
            .Select(c => new ClickData(
                c.ClickedAt,
                c.IpAddress,
                c.UserAgent,
                c.Referer
            ))
            .ToList();

        var topReferrers = clickLogs
            .Where(c => !string.IsNullOrEmpty(c.Referer))
            .GroupBy(c => c.Referer)
            .Select(g => new ReferrerData(g.Key!, g.Count()))
            .OrderByDescending(r => r.Count)
            .Take(10)
            .ToList();

        var hourlyStats = clickLogs
            .GroupBy(c => new DateTime(c.ClickedAt.Year, c.ClickedAt.Month, c.ClickedAt.Day, c.ClickedAt.Hour, 0, 0))
            .Select(g => new HourlyClickData(g.Key, g.Count()))
            .OrderBy(h => h.Hour)
            .ToList();

        var lastClickAt = clickLogs.FirstOrDefault()?.ClickedAt;

        var response = new Response(
            link.Id.ToString(),
            link.ShortCode,
            link.OriginalUrl,
            link.ClickCount,
            link.CreatedAt,
            lastClickAt,
            recentClicks,
            topReferrers,
            hourlyStats
        );

        return Results.Ok(response);
    }
}