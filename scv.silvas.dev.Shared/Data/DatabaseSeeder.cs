using Microsoft.EntityFrameworkCore;
using scv.silvas.dev.Shared.Models;

namespace scv.silvas.dev.Shared.Data;

public static class DatabaseSeeder
{
    public static async Task SeedAsync(AppDbContext db)
    {
        await db.Database.EnsureCreatedAsync();
        await db.Database.ExecuteSqlRawAsync("PRAGMA journal_mode=WAL;");

        // Voeg nieuwe kolommen toe voor bestaande databases (mislukt stilletjes als ze al bestaan)
        try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE TimeEntries ADD COLUMN PhotoData BLOB NULL;"); } catch { }
        try { await db.Database.ExecuteSqlRawAsync("ALTER TABLE TimeEntries ADD COLUMN PhotoMimeType TEXT NULL;"); } catch { }

        if (await db.Users.AnyAsync()) return;

        var users = new List<User>
        {
            new() { Id = 1, Username = "dev1", FullName = "Developer One", Email = "dev1@silvas.nl", CreatedAt = DateTime.Now.AddMonths(-12) },
            new() { Id = 2, Username = "dev2", FullName = "Developer Two", Email = "dev2@silvas.nl", CreatedAt = DateTime.Now.AddMonths(-10) }
        };
        db.Users.AddRange(users);

        var clients = new List<Client>
        {
            new() { Id = 1, Name = "Tech Solutions BV", ContactPerson = "Jan de Vries", Email = "info@techsolutions.nl", Phone = "020-123456" },
            new() { Id = 2, Name = "Digital Marketing Co", ContactPerson = "Marie Jansen", Email = "contact@digitalco.nl", Phone = "010-987654" },
            new() { Id = 3, Name = "E-commerce Plus", ContactPerson = "Peter van den Berg", Email = "support@ecomplus.nl", Phone = "030-555666" }
        };
        db.Clients.AddRange(clients);

        var projects = new List<Project>
        {
            new() { Id = 1, Name = "Website Redesign", Description = "Complete website overhaul", ClientId = 1, StartDate = DateTime.Now.AddMonths(-3), Status = "Active" },
            new() { Id = 2, Name = "Mobile App Development", Description = "iOS and Android app", ClientId = 1, StartDate = DateTime.Now.AddMonths(-2), Status = "Active" },
            new() { Id = 3, Name = "SEO Optimization", Description = "Search engine optimization", ClientId = 2, StartDate = DateTime.Now.AddMonths(-1), Status = "Active" },
            new() { Id = 4, Name = "E-commerce Platform", Description = "Full e-commerce solution", ClientId = 3, StartDate = DateTime.Now.AddDays(-15), Status = "Active" }
        };
        db.Projects.AddRange(projects);

        var today = DateTime.Now;
        var timeEntries = new List<TimeEntry>
        {
            new() { Id = 1, UserId = 1, ProjectId = 1, Date = today.AddDays(-10), Hours = 8, Description = "UI Design", IsApproved = true, CreatedAt = today.AddDays(-10) },
            new() { Id = 2, UserId = 1, ProjectId = 1, Date = today.AddDays(-9), Hours = 7.5m, Description = "Frontend Development", IsApproved = true, CreatedAt = today.AddDays(-9) },
            new() { Id = 3, UserId = 1, ProjectId = 2, Date = today.AddDays(-8), Hours = 8, Description = "iOS Setup", IsApproved = true, CreatedAt = today.AddDays(-8) },
            new() { Id = 4, UserId = 1, ProjectId = 2, Date = today.AddDays(-7), Hours = 6, Description = "API Integration", IsApproved = true, CreatedAt = today.AddDays(-7) },
            new() { Id = 5, UserId = 1, ProjectId = 3, Date = today.AddDays(-6), Hours = 8, Description = "Keyword Research", IsApproved = false, CreatedAt = today.AddDays(-6) },
            new() { Id = 6, UserId = 1, ProjectId = 1, Date = today.AddDays(-5), Hours = 8, Description = "Bug Fixes", IsApproved = true, CreatedAt = today.AddDays(-5) },
            new() { Id = 7, UserId = 1, ProjectId = 4, Date = today.AddDays(-4), Hours = 4, Description = "Database Design", IsApproved = true, CreatedAt = today.AddDays(-4) },
            new() { Id = 8, UserId = 1, ProjectId = 4, Date = today.AddDays(-3), Hours = 8, Description = "Backend Development", IsApproved = true, CreatedAt = today.AddDays(-3) },
            new() { Id = 9, UserId = 1, ProjectId = 1, Date = today.AddDays(-2), Hours = 3, Description = "Testing", IsApproved = false, CreatedAt = today.AddDays(-2) },
            new() { Id = 10, UserId = 1, ProjectId = 2, Date = today.AddDays(-1), Hours = 8, Description = "Android Development", IsApproved = true, CreatedAt = today.AddDays(-1) }
        };
        db.TimeEntries.AddRange(timeEntries);

        var leaveRequests = new List<LeaveRequest>
        {
            new() { Id = 1, UserId = 1, StartDate = today.AddDays(10), EndDate = today.AddDays(14), Reason = "Summer vacation", Status = "Pending", LeaveType = "Vacation", CreatedAt = today },
            new() { Id = 2, UserId = 1, StartDate = today.AddDays(-5), EndDate = today.AddDays(-3), Reason = "Personal reasons", Status = "Approved", LeaveType = "Personal", CreatedAt = today.AddDays(-10), ApprovedBy = "Manager" },
            new() { Id = 3, UserId = 1, StartDate = today.AddDays(25), EndDate = today.AddDays(35), Reason = "Visit family", Status = "Pending", LeaveType = "Vacation", CreatedAt = today.AddDays(-2) }
        };
        db.LeaveRequests.AddRange(leaveRequests);

        await db.SaveChangesAsync();
    }
}
