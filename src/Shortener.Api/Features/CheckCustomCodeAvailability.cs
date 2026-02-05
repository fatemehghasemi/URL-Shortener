using Microsoft.EntityFrameworkCore;
using System.Text.RegularExpressions;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class CheckCustomCodeAvailability
{
    public record Response(bool IsAvailable, string? Message);

    private static readonly string[] ReservedWords = {
        "api", "admin", "www", "app", "help", "support", "about", "contact",
        "terms", "privacy", "login", "register", "dashboard", "profile",
        "settings", "health", "status", "docs", "swagger", "analytics",
        "bulk", "links", "shorten", "custom", "qr"
    };

    public static void MapCheckCustomCodeAvailability(this WebApplication app)
    {
        app.MapGet("/custom/check/{customCode}", Handler)
           .WithName("CheckCustomCodeAvailability")
           .WithSummary("Check if custom code is available")
           .WithDescription("Validates and checks availability of a custom short code");
    }

    private static async Task<IResult> Handler(
        string customCode,
        ShortenerDbContext db)
    {
        if (string.IsNullOrWhiteSpace(customCode))
            return Results.Ok(new Response(false, "Custom code cannot be empty"));

        var validationResult = ValidateCustomCode(customCode);
        if (!validationResult.IsValid)
            return Results.Ok(new Response(false, validationResult.ErrorMessage));

        var exists = await db.Links
            .AnyAsync(l => l.ShortCode == customCode);

        if (exists)
            return Results.Ok(new Response(false, "Custom code is already taken"));

        return Results.Ok(new Response(true, "Custom code is available"));
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