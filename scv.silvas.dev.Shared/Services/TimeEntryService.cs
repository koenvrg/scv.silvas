using Microsoft.EntityFrameworkCore;
using scv.silvas.dev.Shared.Data;
using scv.silvas.dev.Shared.Models;

namespace scv.silvas.dev.Shared.Services;

public class TimeEntryService(IDbContextFactory<AppDbContext> contextFactory)
{
    public async Task<List<TimeEntry>> GetAllTimeEntriesAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.TimeEntries.Include(t => t.Project).Include(t => t.User).ToListAsync();
    }

    public async Task<List<TimeEntry>> GetTimeEntriesByUserAsync(int userId)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.TimeEntries
            .Include(t => t.Project)
            .Where(t => t.UserId == userId)
            .ToListAsync();
    }

    public async Task<List<TimeEntry>> GetTimeEntriesByMonthAsync(int userId, DateTime month)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.TimeEntries
            .Include(t => t.Project)
            .Where(t => t.UserId == userId && t.Date.Year == month.Year && t.Date.Month == month.Month)
            .ToListAsync();
    }

    public async Task<decimal> GetTotalHoursByUserAsync(int userId, DateTime? month = null)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var query = db.TimeEntries.Where(t => t.UserId == userId);
        if (month.HasValue)
            query = query.Where(t => t.Date.Year == month.Value.Year && t.Date.Month == month.Value.Month);
        return await query.SumAsync(t => t.Hours);
    }

    public async Task<TimeEntry> CreateTimeEntryAsync(TimeEntry entry)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        entry.CreatedAt = DateTime.Now;
        db.TimeEntries.Add(entry);
        await db.SaveChangesAsync();
        return entry;
    }

    public async Task<bool> UpdateTimeEntryAsync(TimeEntry entry)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var existing = await db.TimeEntries.FindAsync(entry.Id);
        if (existing == null) return false;
        existing.ProjectId = entry.ProjectId;
        existing.Date = entry.Date;
        existing.Hours = entry.Hours;
        existing.Description = entry.Description;
        existing.PhotoData = entry.PhotoData;
        existing.PhotoMimeType = entry.PhotoMimeType;
        existing.UpdatedAt = DateTime.Now;
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteTimeEntryAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var entry = await db.TimeEntries.FindAsync(id);
        if (entry == null) return false;
        db.TimeEntries.Remove(entry);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<Dictionary<int, decimal>> GetHoursByProjectAsync(int userId, DateTime? month = null)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var query = db.TimeEntries.Where(t => t.UserId == userId);
        if (month.HasValue)
            query = query.Where(t => t.Date.Year == month.Value.Year && t.Date.Month == month.Value.Month);
        return await query.GroupBy(t => t.ProjectId)
            .Select(g => new { g.Key, Hours = g.Sum(t => t.Hours) })
            .ToDictionaryAsync(x => x.Key, x => x.Hours);
    }
}
