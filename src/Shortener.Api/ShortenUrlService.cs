using System.Collections.Concurrent;

public class ShortenUrlService
{
    private readonly ConcurrentDictionary<string, LinkResponse> _links = new();

    // Generate a short code
    private string GenerateCode()
    {
        var chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        var random = new Random();
        return new string(Enumerable.Range(0, 6).Select(_ => chars[random.Next(chars.Length)]).ToArray());
    }

    // Create short link
    public LinkResponse CreateShortLink(GenerateLinkRequest request)
    {
        var code = GenerateCode();
        var link = new LinkResponse(Guid.NewGuid().ToString(), request.DestinationUrl, code, DateTime.UtcNow);
        _links[code] = link;
        return link;
    }

    // Retrieve original URL by short code
    public string? GetDestinationUrl(string shortCode)
    {
        if (_links.TryGetValue(shortCode, out var link))
            return link.DestinationUrl;

        return null;
    }
}
