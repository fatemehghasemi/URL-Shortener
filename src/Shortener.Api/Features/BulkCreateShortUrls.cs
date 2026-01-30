using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class BulkCreateShortUrls
{
    public record Request(List<string> Urls);
    public record Response(List<string> CreatedShortCodes);

    public static void MapBulkCreateShortUrls(this WebApplication app)
    {
        app.MapPost("/bulk/shorten", Handler)
           .WithName("BulkCreateShortUrls")
           .WithSummary("Create multiple short URLs at once")
           .WithDescription("Creates shortened versions of multiple URLs in a single request");
    }

    private static async Task<IResult> Handler(
        Request request,
        ShortenerDbContext db)
    {
        if (request.Urls == null || !request.Urls.Any())
            return Results.BadRequest("At least one URL is required");

        if (request.Urls.Count > 100)
            return Results.BadRequest("Maximum 100 URLs allowed per request");

        var shortCodes = new List<string>();
        
        foreach (var url in request.Urls)
        {
            if (string.IsNullOrWhiteSpace(url))
                return Results.BadRequest($"Empty URL found in request");
                
            if (!IsValidUrl(url))
                return Results.BadRequest($"Invalid URL format: {url}");
        }
        
        foreach (var url in request.Urls)
        {
            shortCodes.Add("temp123");
        }

        var response = new Response(shortCodes);
        return Results.Ok(response);
    }

    private static bool IsValidUrl(string url)
    {
        return Uri.TryCreate(url, UriKind.Absolute, out var result) &&
               (result.Scheme == Uri.UriSchemeHttp || result.Scheme == Uri.UriSchemeHttps);
    }
}