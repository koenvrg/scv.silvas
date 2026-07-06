using scv.silvas.dev.Shared.Models;
using scv.silvas.dev.Shared.Services;

namespace scv.silvas.dev.Tests;

public class TimeEntryServiceTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();
    private readonly TimeEntryService _sut;
    private readonly ClientService _clientService;
    private readonly ProjectService _projectService;

    public TimeEntryServiceTests()
    {
        _sut = new TimeEntryService(_factory);
        _clientService = new ClientService(_factory);
        _projectService = new ProjectService(_factory);

        using var db = _factory.CreateDbContext();
        db.Users.AddRange(
            new User { Id = 1, Username = "dev1", FullName = "Developer One" },
            new User { Id = 2, Username = "dev2", FullName = "Developer Two" });
        db.SaveChanges();
    }

    private async Task<Project> CreateProjectAsync(string name = "Project")
    {
        var client = await _clientService.CreateClientAsync(new Client { Name = "Client" });
        return await _projectService.CreateProjectAsync(new Project { Name = name, ClientId = client.Id });
    }

    [Fact]
    public async Task CreateTimeEntryAsync_SetsCreatedAt()
    {
        var project = await CreateProjectAsync();

        var entry = await _sut.CreateTimeEntryAsync(new TimeEntry
        {
            UserId = 1,
            ProjectId = project.Id,
            Date = DateTime.Today,
            Hours = 8
        });

        Assert.NotEqual(default, entry.CreatedAt);
    }

    [Fact]
    public async Task GetTotalHoursByUserAsync_SumsHoursForUser()
    {
        var project = await CreateProjectAsync();
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = DateTime.Today, Hours = 4 });
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = DateTime.Today, Hours = 3.5m });
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 2, ProjectId = project.Id, Date = DateTime.Today, Hours = 100 });

        var total = await _sut.GetTotalHoursByUserAsync(1);

        Assert.Equal(7.5m, total);
    }

    [Fact]
    public async Task GetTotalHoursByUserAsync_FiltersByMonth_WhenProvided()
    {
        var project = await CreateProjectAsync();
        var thisMonth = new DateTime(2026, 7, 1);
        var lastMonth = new DateTime(2026, 6, 1);
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = thisMonth, Hours = 5 });
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = lastMonth, Hours = 20 });

        var total = await _sut.GetTotalHoursByUserAsync(1, thisMonth);

        Assert.Equal(5m, total);
    }

    [Fact]
    public async Task GetTimeEntriesByMonthAsync_ReturnsOnlyEntriesInThatMonth()
    {
        var project = await CreateProjectAsync();
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = new DateTime(2026, 7, 5), Hours = 8 });
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = new DateTime(2026, 6, 5), Hours = 8 });

        var result = await _sut.GetTimeEntriesByMonthAsync(1, new DateTime(2026, 7, 1));

        Assert.Single(result);
        Assert.Equal(7, result[0].Date.Month);
    }

    [Fact]
    public async Task UpdateTimeEntryAsync_UpdatesFieldsAndTimestamp()
    {
        var project = await CreateProjectAsync();
        var entry = await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = DateTime.Today, Hours = 4, Description = "Old" });

        entry.Hours = 6;
        entry.Description = "New";
        var updated = await _sut.UpdateTimeEntryAsync(entry);

        Assert.True(updated);
        var all = await _sut.GetTimeEntriesByUserAsync(1);
        var fetched = Assert.Single(all);
        Assert.Equal(6, fetched.Hours);
        Assert.Equal("New", fetched.Description);
        Assert.NotNull(fetched.UpdatedAt);
    }

    [Fact]
    public async Task UpdateTimeEntryAsync_ReturnsFalse_WhenEntryDoesNotExist()
    {
        var updated = await _sut.UpdateTimeEntryAsync(new TimeEntry { Id = 999 });

        Assert.False(updated);
    }

    [Fact]
    public async Task DeleteTimeEntryAsync_RemovesEntry()
    {
        var project = await CreateProjectAsync();
        var entry = await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = project.Id, Date = DateTime.Today, Hours = 4 });

        var deleted = await _sut.DeleteTimeEntryAsync(entry.Id);

        Assert.True(deleted);
        Assert.Empty(await _sut.GetTimeEntriesByUserAsync(1));
    }

    [Fact]
    public async Task GetHoursByProjectAsync_GroupsHoursPerProject()
    {
        var projectA = await CreateProjectAsync("A");
        var projectB = await CreateProjectAsync("B");
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = projectA.Id, Date = DateTime.Today, Hours = 3 });
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = projectA.Id, Date = DateTime.Today, Hours = 2 });
        await _sut.CreateTimeEntryAsync(new TimeEntry { UserId = 1, ProjectId = projectB.Id, Date = DateTime.Today, Hours = 4 });

        var result = await _sut.GetHoursByProjectAsync(1);

        Assert.Equal(5m, result[projectA.Id]);
        Assert.Equal(4m, result[projectB.Id]);
    }

    public void Dispose() => _factory.Dispose();
}
