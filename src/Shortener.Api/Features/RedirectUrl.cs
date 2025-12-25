using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;
using Shortener.Api.Entities;

namespace Shortener.Api.Features;

public static class RedirectUrl
{
    public static void MapRedirectUrl(this WebApplication app)
    {
        app.MapGet("/{shortCode}", Handler)
           .WithName("RedirectUrl")
           .WithSummary("Redirect to original URL")
           .WithDescription("Redirects to the original URL using the short code");
    }

    private static async Task<IResult> Handler(
        string shortCode,
        ShortenerDbContext db,
        HttpContext context)
    {
        var link = await db.Links
            .FirstOrDefaultAsync(l => l.ShortCode == shortCode && l.IsActive);

        if (link == null)
            return Results.NotFound("Short URL not found");

        if (link.ExpiresAt.HasValue && link.ExpiresAt < DateTime.UtcNow)
            return Results.NotFound("Short URL has expired");

        link.ClickCount++;

        var clickLog = new ClickLog
        {
            Id = Guid.NewGuid(),
            LinkId = link.Id,
            IpAddress = GetClientIpAddress(context),
            UserAgent = context.Request.Headers.UserAgent.ToString(),
            Referer = context.Request.Headers.Referer.ToString(),
            ClickedAt = DateTime.UtcNow
        };

        db.ClickLogs.Add(clickLog);
        await db.SaveChangesAsync();

        return Results.Redirect(link.OriginalUrl);
    }

    private static string? GetClientIpAddress(HttpContext context)
    {
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString();
    }
}