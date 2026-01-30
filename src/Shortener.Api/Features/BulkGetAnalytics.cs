using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class BulkGetAnalytics
{
    public record Request(List<string> ShortCodes);
    public record Response(List<string> FoundCodes);

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

        var foundCodes = new List<string>();
        
        foreach (var shortCode in request.ShortCodes)
        {
            foundCodes.Add(shortCode);
        }

        var response = new Response(foundCodes);
        return Results.Ok(response);
    }
}