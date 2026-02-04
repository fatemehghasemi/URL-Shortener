using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Shortener.Api.Data;
using Shortener.Api.Entities;

namespace Shortener.Api.Features;

public static class CreateCustomShortUrl
{
    public record Request(string Url, string CustomCode);
    public record Response(string Id, string OriginalUrl, string ShortCode, string ShortUrl, DateTime CreatedAt);

    private static readonly string[] ReservedWords = {
        "api", "admin", "www", "app", "help", "support", "about", "contact",
        "terms", "privacy", "login", "register", "dashboard", "profile",
        "settings", "health", "status", "docs", "swagger", "analytics",
        "bulk", "links", "shorten", "custom", "qr"
    };

    public static void MapCreateCustomShortUrl(this WebApplication app)
    {
        app.MapPost("/custom/shorten", Handler)
           .WithName("CreateCustomShortUrl")
           .WithSummary("Create a short URL with custom code")
           .WithDescription("Creates a shortened URL with a user-defined short code");
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

        if (string.IsNullOrWhiteSpace(request.CustomCode))
            return Results.BadRequest("Custom code is required");

        var validationResult = ValidateCustomCode(request.CustomCode);
        if (!validationResult.IsValid)
            return Results.BadRequest(validationResult.ErrorMessage);

        var existingLink = await db.Links
            .FirstOrDefaultAsync(l => l.ShortCode == request.CustomCode);

        if (existingLink != null)
            return Results.Conflict("Custom code is already taken");

        var link = new Link
        {
            Id = Guid.NewGuid(),
            OriginalUrl = request.Url,
            ShortCode = request.CustomCode,
            CreatedAt = DateTime.UtcNow,
            IsActive = true,
            ClickCount = 0
        };

        db.Links.Add(link);
        await db.SaveChangesAsync();

        var baseUrl = config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var shortUrl = $"{baseUrl}/{request.CustomCode}";

        var response = new Response(
            link.Id.ToString(),
            link.OriginalUrl,
            link.ShortCode,
            shortUrl,
            link.CreatedAt
        );

        return Results.Created($"/links/{link.Id}", response);
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }

    private static ValidationResult ValidateCustomCode(string customCode)
    {
        if (customCode.Length < 3)
            return new ValidationResult(false, "Custom code must be at least 3 characters long");

        if (customCode.Length > 20)
            return new ValidationResult(false, "Custom code must be no more than 20 characters long");

        if (!Regex.IsMatch(customCode, @"^[a-zA-Z0-9_-]+$"))
            return new ValidationResult(false, "Custom code can only contain letters, numbers, hyphens, and underscores");

        if (customCode.StartsWith("-") || customCode.EndsWith("-"))
            return new ValidationResult(false, "Custom code cannot start or end with a hyphen");

        if (customCode.StartsWith("_") || customCode.EndsWith("_"))
            return new ValidationResult(false, "Custom code cannot start or end with an underscore");

        if (ReservedWords.Contains(customCode.ToLower()))
            return new ValidationResult(false, "This custom code is reserved and cannot be used");

        return new ValidationResult(true, null);
    }

    private record ValidationResult(bool IsValid, string? ErrorMessage);
}