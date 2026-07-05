namespace scv.silvas.dev.Shared.Models;

public class TimeEntry
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public int ProjectId { get; set; }
    public Project? Project { get; set; }
    public DateTime Date { get; set; }
    public decimal Hours { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool IsApproved { get; set; }
    public byte[]? PhotoData { get; set; }
    public string? PhotoMimeType { get; set; }
}
