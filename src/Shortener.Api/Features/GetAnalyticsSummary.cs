using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class GetAnalyticsSummary
{
    public record Response(
        int TotalLinks,
        int ActiveLinks,
        int TotalClicks,
        int ClicksToday,
        int ClicksThisWeek,
        List<TopLinkData> TopLinks,
        List<DailyClickData> DailyStats
    );

    public record TopLinkData(
        string ShortCode,
        string OriginalUrl,
        int Clicks,
        DateTime CreatedAt
    );

    public record DailyClickData(
        DateTime Date,
        int Clicks
    );

    public static void MapGetAnalyticsSummary(this WebApplication app)
    {
        app.MapGet("/analytics/summary", Handler)
           .WithName("GetAnalyticsSummary")
           .WithSummary("Get overall analytics summary")
           .WithDescription("Returns system-wide analytics including total links, clicks, and trends");
    }

    private static async Task<IResult> Handler(ShortenerDbContext db)
    {
        var now = DateTime.UtcNow;
        var today = now.Date;
        var weekAgo = today.AddDays(-7);

        var totalLinks = await db.Links.CountAsync();
        var activeLinks = await db.Links.CountAsync(l => l.IsActive);
        var totalClicks = await db.ClickLogs.CountAsync();

        var clicksToday = await db.ClickLogs
            .CountAsync(c => c.ClickedAt >= today);

        var clicksThisWeek = await db.ClickLogs
            .CountAsync(c => c.ClickedAt >= weekAgo);

        var topLinks = await db.Links
            .Where(l => l.IsActive && l.ClickCount > 0)
            .OrderByDescending(l => l.ClickCount)
            .Take(10)
            .Select(l => new TopLinkData(
                l.ShortCode,
                l.OriginalUrl,
                l.ClickCount,
                l.CreatedAt
            ))
            .ToListAsync();

        var dailyStats = await db.ClickLogs
            .Where(c => c.ClickedAt >= weekAgo)
            .GroupBy(c => c.ClickedAt.Date)
            .Select(g => new DailyClickData(g.Key, g.Count()))
            .OrderBy(d => d.Date)
            .ToListAsync();

        var response = new Response(
            totalLinks,
            activeLinks,
            totalClicks,
            clicksToday,
            clicksThisWeek,
            topLinks,
            dailyStats
        );

        return Results.Ok(response);
    }
}