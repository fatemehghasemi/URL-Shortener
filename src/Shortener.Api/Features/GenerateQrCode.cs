using Microsoft.EntityFrameworkCore;
using QRCoder;
using Shortener.Api.Data;
using Shortener.Api.Entities;

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
        var link = await GetValidLinkAsync(db, shortCode);
        if (link is null)
            return Results.NotFound("Short URL not found");

        if (IsExpired(link))
            return Results.StatusCode(StatusCodes.Status410Gone);

        var qrSize = GetQrSizeFromQuery(context.Request.Query["size"]);

        var baseUrl = GetBaseUrl(config);
        var shortUrl = $"{baseUrl}/{shortCode}";

        try
        {
            var qrBytes = GenerateQrCodeBytes(shortUrl, qrSize);
            return Results.File(qrBytes, "image/png", $"qr-code-{shortCode}.png");
        }
        catch
        {
            return Results.Problem("Failed to generate QR code");
        }
    }

    private static async Task<Link?> GetValidLinkAsync(
        ShortenerDbContext db,
        string shortCode)
    {
        return await db.Links
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.ShortCode == shortCode && l.IsActive);
    }

    private static bool IsExpired(Link link)
    {
        var now = DateTime.UtcNow;
        return link.ExpiresAt.HasValue && link.ExpiresAt < now;
    }

    private static byte[] GenerateQrCodeBytes(string content, int size)
    {
        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(content, QRCodeGenerator.ECCLevel.Q);

        var qrCode = new PngByteQRCode(qrCodeData);
        return qrCode.GetGraphic(size);
    }

    private static int GetQrSizeFromQuery(string? sizeParam)
    {
        if (!int.TryParse(sizeParam, out var size))
            return 10;

        return Math.Clamp(size, 5, 50);
    }

    private static string GetBaseUrl(IConfiguration config)
    {
        var baseUrl = config["AppSettings:BaseUrl"];
        if (string.IsNullOrWhiteSpace(baseUrl))
            throw new InvalidOperationException("BaseUrl is not configured");

        return baseUrl.TrimEnd('/');
    }
}
