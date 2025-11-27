using System.Text.Json.Serialization;

public record GenerateLinkRequest
{
    [JsonConstructor]
    public GenerateLinkRequest(string destinationUrl)
    {
        DestinationUrl = destinationUrl;
    }

    public string DestinationUrl { get; init; }
}
