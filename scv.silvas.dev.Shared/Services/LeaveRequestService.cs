using Microsoft.EntityFrameworkCore;
using scv.silvas.dev.Shared.Data;
using scv.silvas.dev.Shared.Models;

namespace scv.silvas.dev.Shared.Services;

public class LeaveRequestService(IDbContextFactory<AppDbContext> contextFactory)
{
    public async Task<List<LeaveRequest>> GetAllLeaveRequestsAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.LeaveRequests.Include(lr => lr.User).ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetLeaveRequestsByUserAsync(int userId)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.LeaveRequests.Where(lr => lr.UserId == userId).ToListAsync();
    }

    public async Task<List<LeaveRequest>> GetPendingLeaveRequestsAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.LeaveRequests.Include(lr => lr.User).Where(lr => lr.Status == "Pending").ToListAsync();
    }

    public async Task<LeaveRequest> CreateLeaveRequestAsync(LeaveRequest request)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        request.CreatedAt = DateTime.Now;
        request.Status = "Pending";
        db.LeaveRequests.Add(request);
        await db.SaveChangesAsync();
        return request;
    }

    public async Task<bool> UpdateLeaveRequestAsync(LeaveRequest request)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var existing = await db.LeaveRequests.FindAsync(request.Id);
        if (existing == null) return false;
        existing.StartDate = request.StartDate;
        existing.EndDate = request.EndDate;
        existing.Reason = request.Reason;
        existing.LeaveType = request.LeaveType;
        existing.UpdatedAt = DateTime.Now;
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> ApproveLeaveRequestAsync(int id, string approvedBy)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var request = await db.LeaveRequests.FindAsync(id);
        if (request == null) return false;
        request.Status = "Approved";
        request.ApprovedBy = approvedBy;
        request.UpdatedAt = DateTime.Now;
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> RejectLeaveRequestAsync(int id, string reason)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var request = await db.LeaveRequests.FindAsync(id);
        if (request == null) return false;
        request.Status = "Rejected";
        request.RejectionReason = reason;
        request.UpdatedAt = DateTime.Now;
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteLeaveRequestAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var request = await db.LeaveRequests.FindAsync(id);
        if (request == null) return false;
        db.LeaveRequests.Remove(request);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<int> GetTotalLeaveDaysAsync(int userId, DateTime year)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var requests = await db.LeaveRequests
            .Where(lr => lr.UserId == userId && lr.StartDate.Year == year.Year && lr.Status == "Approved")
            .ToListAsync();
        return requests.Sum(lr => lr.GetTotalDays());
    }
}
