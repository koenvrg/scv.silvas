using Microsoft.EntityFrameworkCore;
using scv.silvas.dev.Shared.Data;
using scv.silvas.dev.Shared.Models;

namespace scv.silvas.dev.Shared.Services;

public class UserService(IDbContextFactory<AppDbContext> contextFactory)
{
    private User? _currentUser;

    public async Task<User?> AuthenticateAsync(string username, string password)
    {
        if (username != password) return null;

        await using var db = await contextFactory.CreateDbContextAsync();
        _currentUser = await db.Users.FirstOrDefaultAsync(u => u.Username == username && u.IsActive);
        return _currentUser;
    }

    public User? GetCurrentUser() => _currentUser;

    public async Task<User?> GetUserByIdAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Users.FindAsync(id);
    }

    public async Task<List<User>> GetAllUsersAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Users.ToListAsync();
    }

    public bool IsCurrentUserAdmin() => _currentUser?.Username == "dev1";

    public void Logout() => _currentUser = null;
}
