using Microsoft.EntityFrameworkCore;
using scv.silvas.dev.Shared.Data;
using scv.silvas.dev.Shared.Models;

namespace scv.silvas.dev.Shared.Services;

public class ProjectService(IDbContextFactory<AppDbContext> contextFactory)
{
    public async Task<List<Project>> GetAllProjectsAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Projects.Include(p => p.Client).ToListAsync();
    }

    public async Task<Project?> GetProjectByIdAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Projects.Include(p => p.Client).FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<List<Project>> GetProjectsByClientAsync(int clientId)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Projects.Where(p => p.ClientId == clientId).ToListAsync();
    }

    public async Task<Project> CreateProjectAsync(Project project)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Projects.Add(project);
        await db.SaveChangesAsync();
        return project;
    }

    public async Task<bool> UpdateProjectAsync(Project project)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Projects.Update(project);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteProjectAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var project = await db.Projects.FindAsync(id);
        if (project == null) return false;
        db.Projects.Remove(project);
        return await db.SaveChangesAsync() > 0;
    }
}
