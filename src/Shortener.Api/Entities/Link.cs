namespace Shortener.Api.Entities;

public class Link
{
    public Guid Id { get; set; }
    public string OriginalUrl { get; set; } = string.Empty;
    public string ShortCode { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ExpiresAt { get; set; }
    public bool IsActive { get; set; } = true;
    public int ClickCount { get; set; } = 0;
    
    public ICollection<ClickLog> ClickLogs { get; set; } = new List<ClickLog>();
}