using scv.silvas.dev.Shared.Models;
using scv.silvas.dev.Shared.Services;

namespace scv.silvas.dev.Tests;

public class ProjectServiceTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();
    private readonly ProjectService _sut;
    private readonly ClientService _clientService;

    public ProjectServiceTests()
    {
        _sut = new ProjectService(_factory);
        _clientService = new ClientService(_factory);
    }

    [Fact]
    public async Task CreateProjectAsync_PersistsProject()
    {
        var client = await _clientService.CreateClientAsync(new Client { Name = "Client A" });

        var project = await _sut.CreateProjectAsync(new Project { Name = "Website", ClientId = client.Id });

        Assert.NotEqual(0, project.Id);
        var fetched = await _sut.GetProjectByIdAsync(project.Id);
        Assert.NotNull(fetched);
        Assert.Equal("Website", fetched!.Name);
    }

    [Fact]
    public async Task GetProjectByIdAsync_IncludesClient()
    {
        var client = await _clientService.CreateClientAsync(new Client { Name = "Client A" });
        var project = await _sut.CreateProjectAsync(new Project { Name = "Website", ClientId = client.Id });

        var fetched = await _sut.GetProjectByIdAsync(project.Id);

        Assert.NotNull(fetched!.Client);
        Assert.Equal("Client A", fetched.Client!.Name);
    }

    [Fact]
    public async Task GetProjectsByClientAsync_ReturnsOnlyMatchingProjects()
    {
        var clientA = await _clientService.CreateClientAsync(new Client { Name = "Client A" });
        var clientB = await _clientService.CreateClientAsync(new Client { Name = "Client B" });
        await _sut.CreateProjectAsync(new Project { Name = "Project A1", ClientId = clientA.Id });
        await _sut.CreateProjectAsync(new Project { Name = "Project A2", ClientId = clientA.Id });
        await _sut.CreateProjectAsync(new Project { Name = "Project B1", ClientId = clientB.Id });

        var result = await _sut.GetProjectsByClientAsync(clientA.Id);

        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.Equal(clientA.Id, p.ClientId));
    }

    [Fact]
    public async Task DeleteProjectAsync_RemovesProject()
    {
        var client = await _clientService.CreateClientAsync(new Client { Name = "Client A" });
        var project = await _sut.CreateProjectAsync(new Project { Name = "Website", ClientId = client.Id });

        var deleted = await _sut.DeleteProjectAsync(project.Id);

        Assert.True(deleted);
        Assert.Null(await _sut.GetProjectByIdAsync(project.Id));
    }

    public void Dispose() => _factory.Dispose();
}
