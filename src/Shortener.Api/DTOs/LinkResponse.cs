public record LinkResponse(
    string Id,
    string DestinationUrl,
    string ShortenCode,
    DateTime CreatedOn
)
{
    public LinkResponse(string destinationUrl, string shortenCode)
        : this(Guid.NewGuid().ToString(), destinationUrl, shortenCode, DateTime.UtcNow) { }
}