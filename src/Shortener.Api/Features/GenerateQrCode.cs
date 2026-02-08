using Microsoft.EntityFrameworkCore;
using QRCoder;
using Shortener.Api.Data;

namespace Shortener.Api.Features;

public static class GenerateQrCode
{
    public static void MapGenerateQrCode(this WebApplication app)
    {
        app.MapGet("/qr/{shortCode}", Handler)
           .WithName("GenerateQrCode")
           .WithSummary("Generate QR code for short URL")
           .WithDescription("Generates a QR code PNG image for the specified short code");
    }

    private static async Task<IResult> Handler(
        string shortCode,
        ShortenerDbContext db,
        IConfiguration config,
        HttpContext context)
    {
        var link = await db.Links
            .FirstOrDefaultAsync(l => l.ShortCode == shortCode && l.IsActive);

        if (link == null)
            return Results.NotFound("Short URL not found");

        if (link.ExpiresAt.HasValue && link.ExpiresAt < DateTime.UtcNow)
            return Results.NotFound("Short URL has expired");

        var query = context.Request.Query;

        var size = int.TryParse(query["size"], out var parsedSize)
            ? Math.Clamp(parsedSize, 5, 50)
            : 10;

        var baseUrl = config["AppSettings:BaseUrl"] ?? "http://localhost:5000";
        var shortUrl = $"{baseUrl}/{shortCode}";

        try
        {
            var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(shortUrl, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeBytes = qrCode.GetGraphic(size);
            
            return Results.File(qrCodeBytes, "image/png", $"qr-code-{shortCode}.png");
        }
        catch (Exception ex)
        {
            return Results.Problem($"Failed to generate QR code: {ex.Message}");
        }
    }
}