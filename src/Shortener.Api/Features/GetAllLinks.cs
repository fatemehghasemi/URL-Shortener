using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class GetAllLinks
{
    public record Response(
        List<LinkData> Links,
        int TotalCount,
        int Page,
        int PageSize
    );

    public record LinkData(
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

    public static void MapGetAllLinks(this WebApplication app)
    {
        app.MapGet("/links", Handler)
           .WithName("GetAllLinks")
           .WithSummary("Get all links with pagination")
           .WithDescription("Returns paginated list of all created links with basic statistics");
    }

    private static async Task<IResult> Handler(
        ShortenerDbContext db,
        IConfiguration config,
        int page = 1,
        int pageSize = 20)
    {
        if (page < 1) page = 1;
        if (pageSize < 1 || pageSize > 100) pageSize = 20;

        var totalCount = await db.Links.CountAsync();
        var skip = (page - 1) * pageSize;

        var links = await db.Links
            .OrderByDescending(l => l.CreatedAt)
            .Skip(skip)
            .Take(pageSize)
            .Select(l => new LinkData(
                l.Id.ToString(),
                l.ShortCode,
                l.OriginalUrl,
                $"{config["AppSettings:BaseUrl"] ?? "http://localhost:5000"}/{l.ShortCode}",
                l.ClickCount,
                l.CreatedAt,
                l.ExpiresAt,
                l.IsActive,
                db.ClickLogs
                    .Where(c => c.LinkId == l.Id)
                    .OrderByDescending(c => c.ClickedAt)
                    .Select(c => c.ClickedAt)
                    .FirstOrDefault()
            ))
            .ToListAsync();

        var response = new Response(
            links,
            totalCount,
            page,
            pageSize
        );

        return Results.Ok(response);
    }
}