using scv.silvas.dev.Shared.Models;
using scv.silvas.dev.Shared.Services;

namespace scv.silvas.dev.Tests;

public class UserServiceTests : IDisposable
{
    private readonly SqliteInMemoryContextFactory _factory = new();
    private readonly UserService _sut;

    public UserServiceTests()
    {
        _sut = new UserService(_factory);
    }

    private async Task SeedUserAsync(string username = "dev1", bool isActive = true)
    {
        await using var db = _factory.CreateDbContext();
        db.Users.Add(new User { Username = username, FullName = username, IsActive = isActive });
        await db.SaveChangesAsync();
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsUser_WhenUsernameEqualsPasswordAndUserIsActive()
    {
        await SeedUserAsync("dev1");

        var user = await _sut.AuthenticateAsync("dev1", "dev1");

        Assert.NotNull(user);
        Assert.Equal("dev1", user!.Username);
        Assert.Same(user, _sut.GetCurrentUser());
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenUsernameAndPasswordDiffer()
    {
        await SeedUserAsync("dev1");

        var user = await _sut.AuthenticateAsync("dev1", "wrong");

        Assert.Null(user);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenUserIsInactive()
    {
        await SeedUserAsync("dev1", isActive: false);

        var user = await _sut.AuthenticateAsync("dev1", "dev1");

        Assert.Null(user);
    }

    [Fact]
    public async Task AuthenticateAsync_ReturnsNull_WhenUserDoesNotExist()
    {
        var user = await _sut.AuthenticateAsync("ghost", "ghost");

        Assert.Null(user);
    }

    [Fact]
    public async Task IsCurrentUserAdmin_ReturnsTrue_OnlyForDev1()
    {
        await SeedUserAsync("dev1");
        await _sut.AuthenticateAsync("dev1", "dev1");

        Assert.True(_sut.IsCurrentUserAdmin());
    }

    [Fact]
    public async Task IsCurrentUserAdmin_ReturnsFalse_ForOtherUsers()
    {
        await SeedUserAsync("dev2");
        await _sut.AuthenticateAsync("dev2", "dev2");

        Assert.False(_sut.IsCurrentUserAdmin());
    }

    [Fact]
    public async Task Logout_ClearsCurrentUser()
    {
        await SeedUserAsync("dev1");
        await _sut.AuthenticateAsync("dev1", "dev1");

        _sut.Logout();

        Assert.Null(_sut.GetCurrentUser());
        Assert.False(_sut.IsCurrentUserAdmin());
    }

    public void Dispose() => _factory.Dispose();
}
