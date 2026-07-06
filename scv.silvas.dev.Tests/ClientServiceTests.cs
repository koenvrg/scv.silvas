using scv.silvas.dev.Shared.Models;
using scv.silvas.dev.Shared.Services;

namespace scv.silvas.dev.Tests;

public class ClientServiceTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();
    private readonly ClientService _sut;

    public ClientServiceTests()
    {
        _sut = new ClientService(_factory);
    }

    [Fact]
    public async Task CreateClientAsync_PersistsClient()
    {
        var client = await _sut.CreateClientAsync(new Client { Name = "Acme BV", Email = "info@acme.nl" });

        Assert.NotEqual(0, client.Id);
        var all = await _sut.GetAllClientsAsync();
        Assert.Single(all);
        Assert.Equal("Acme BV", all[0].Name);
    }

    [Fact]
    public async Task GetClientByIdAsync_ReturnsNull_WhenNotFound()
    {
        var result = await _sut.GetClientByIdAsync(999);

        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateClientAsync_UpdatesExistingClient()
    {
        var client = await _sut.CreateClientAsync(new Client { Name = "Old Name" });

        client.Name = "New Name";
        var updated = await _sut.UpdateClientAsync(client);

        Assert.True(updated);
        var fetched = await _sut.GetClientByIdAsync(client.Id);
        Assert.Equal("New Name", fetched!.Name);
    }

    [Fact]
    public async Task DeleteClientAsync_RemovesClient()
    {
        var client = await _sut.CreateClientAsync(new Client { Name = "To Delete" });

        var deleted = await _sut.DeleteClientAsync(client.Id);

        Assert.True(deleted);
        Assert.Null(await _sut.GetClientByIdAsync(client.Id));
    }

    [Fact]
    public async Task DeleteClientAsync_ReturnsFalse_WhenClientDoesNotExist()
    {
        var deleted = await _sut.DeleteClientAsync(12345);

        Assert.False(deleted);
    }

    public void Dispose() => _factory.Dispose();
}
