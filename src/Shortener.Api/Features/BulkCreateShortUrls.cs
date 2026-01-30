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

        var shortCodes = new List<string>();
        
        // TODO: Implement bulk URL creation logic
        foreach (var url in request.Urls)
        {
            shortCodes.Add("temp123"); // Placeholder
        }

        var response = new Response(shortCodes);
        return Results.Ok(response);
    }
}