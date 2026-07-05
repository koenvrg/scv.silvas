namespace scv.silvas.dev.Shared.Models;

public class LeaveRequest
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public string Reason { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public string LeaveType { get; set; } = "Vacation";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public string? ApprovedBy { get; set; }
    public string? RejectionReason { get; set; }

    public int GetTotalDays() => (int)(EndDate - StartDate).TotalDays + 1;
}
