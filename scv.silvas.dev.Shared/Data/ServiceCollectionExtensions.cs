using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using scv.silvas.dev.Shared.Services;

namespace scv.silvas.dev.Shared.Data;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddScvServices(this IServiceCollection services, string dbPath)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(dbPath)!);

        services.AddDbContextFactory<AppDbContext>(options =>
            options.UseSqlite($"Data Source={dbPath}"));

        services.AddScoped<UserService>();
        services.AddScoped<ClientService>();
        services.AddScoped<ProjectService>();
        services.AddScoped<TimeEntryService>();
        services.AddScoped<LeaveRequestService>();

        return services;
    }

    public static async Task SeedDatabaseAsync(this IServiceProvider services)
    {
        var factory = services.GetRequiredService<IDbContextFactory<AppDbContext>>();
        await using var db = await factory.CreateDbContextAsync();
        await DatabaseSeeder.SeedAsync(db);
    }

    public static void SeedDatabase(this IServiceProvider services)
    {
        SeedDatabaseAsync(services).GetAwaiter().GetResult();
    }
}
