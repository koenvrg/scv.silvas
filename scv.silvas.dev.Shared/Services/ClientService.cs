using Microsoft.EntityFrameworkCore;
using scv.silvas.dev.Shared.Data;
using scv.silvas.dev.Shared.Models;

namespace scv.silvas.dev.Shared.Services;

public class ClientService(IDbContextFactory<AppDbContext> contextFactory)
{
    public async Task<List<Client>> GetAllClientsAsync()
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Clients.ToListAsync();
    }

    public async Task<Client?> GetClientByIdAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        return await db.Clients.FindAsync(id);
    }

    public async Task<Client> CreateClientAsync(Client client)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Clients.Add(client);
        await db.SaveChangesAsync();
        return client;
    }

    public async Task<bool> UpdateClientAsync(Client client)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        db.Clients.Update(client);
        return await db.SaveChangesAsync() > 0;
    }

    public async Task<bool> DeleteClientAsync(int id)
    {
        await using var db = await contextFactory.CreateDbContextAsync();
        var client = await db.Clients.FindAsync(id);
        if (client == null) return false;
        db.Clients.Remove(client);
        return await db.SaveChangesAsync() > 0;
    }
}
