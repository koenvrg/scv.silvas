using Bunit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using scv.silvas.dev.Shared.Components.Pages;
using scv.silvas.dev.Shared.Data;
using scv.silvas.dev.Shared.Models;
using scv.silvas.dev.Shared.Services;
using scv.silvas.dev.UITests.Infrastructure;

namespace scv.silvas.dev.UITests;

public class LoginTests : BunitContext
{
    private readonly SqliteInMemoryContextFactory _factory = new();

    public LoginTests()
    {
        Services.AddSingleton<IDbContextFactory<AppDbContext>>(_factory);
        Services.AddScoped<UserService>();
    }

    private async Task SeedUserAsync(string username)
    {
        await using var db = _factory.CreateDbContext();
        db.Users.Add(new User { Username = username, FullName = username, IsActive = true });
        await db.SaveChangesAsync();
    }

    [Fact]
    public void RendersLoginForm()
    {
        var cut = Render<Login>();

        Assert.Contains("Inloggen", cut.Markup);
        Assert.Contains("Gebruikersnaam", cut.Markup);
    }

    [Fact]
    public async Task InvalidCredentials_ShowsErrorAndDoesNotNavigate()
    {
        await SeedUserAsync("dev1");
        var cut = Render<Login>();
        var nav = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        cut.Find("input[type=text]").Change("dev1");
        cut.Find("input[type=password]").Change("wrong-password");
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        cut.WaitForAssertion(() => Assert.Contains("Ongeldige gebruikersnaam of wachtwoord", cut.Markup));
        Assert.DoesNotContain("/dashboard", nav.Uri);
    }

    [Fact]
    public async Task ValidCredentials_NavigatesToDashboard()
    {
        await SeedUserAsync("dev1");
        var cut = Render<Login>();
        var nav = Services.GetRequiredService<Microsoft.AspNetCore.Components.NavigationManager>();

        cut.Find("input[type=text]").Change("dev1");
        cut.Find("input[type=password]").Change("dev1");
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        cut.WaitForAssertion(() => Assert.EndsWith("/dashboard", nav.Uri));
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) _factory.Dispose();
        base.Dispose(disposing);
    }
}
