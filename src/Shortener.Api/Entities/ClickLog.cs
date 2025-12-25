namespace Shortener.Api.Entities;

public class ClickLog
{
    public Guid Id { get; set; }
    public Guid LinkId { get; set; }
    public string? IpAddress { get; set; }
    public string? UserAgent { get; set; }
    public string? Referer { get; set; }
    public DateTime ClickedAt { get; set; }
    
    public Link Link { get; set; } = null!;
}