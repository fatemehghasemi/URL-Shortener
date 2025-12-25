using Microsoft.EntityFrameworkCore;
using Shortener.Api.Data;
using Shortener.Api.Entities;

namespace Shortener.Api.Features;

public static class CreateShortUrl
{
    public record Request(string Url);
    public record Response(string Id, string OriginalUrl, string ShortCode, string ShortUrl, DateTime CreatedAt);

    public static void MapCreateShortUrl(this WebApplication app)
    {
        app.MapPost("/shorten", Handler)
           .WithName("CreateShortUrl")
           .WithSummary("Create a short URL")
           .WithDescription("Creates a shortened version of the provided URL");
    }

    private static async Task<IResult> Handler(
        Request request,
        ShortenerDbContext db,
        IConfiguration config)
    {
        if (string.IsNullOrWhiteSpace(request.Url))
            return Results.BadRequest("URL is required");

        if (!IsValidUrl(request.Url))
            return Results.BadRequest("Invalid URL format");

        var shortCode = GenerateShortCode();
        
        while (await db.Links.AnyAsync(l => l.ShortCode == shortCode))
        {
            shortCode = GenerateShortCode();
        }

        var link = new Link
        {
            Id = Guid.NewGuid(),
            OriginalUrl = request.Url,
            ShortCode = shortCode,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ClickCount = 0
        };

        db.Links.Add(link);
        await db.SaveChangesAsync();

        var baseUrl = config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var shortUrl = $"{baseUrl}/{shortCode}";

        var response = new Response(
            link.Id.ToString(),
            link.OriginalUrl,
            link.ShortCode,
            shortUrl,
            link.CreatedAt
        );

        return Results.Created($"/links/{link.Id}", response);
    }

    private static string GenerateShortCode()
    {
        const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}