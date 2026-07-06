using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using scv.silvas.dev.Shared.Data;

namespace scv.silvas.dev.UITests.Infrastructure;

/// <summary>
/// Elke test krijgt een eigen in-memory Sqlite-database, zodat UI-tests geïsoleerd
/// en herhaalbaar zijn maar wel tegen dezelfde database-engine draaien als productie.
/// </summary>
public sealed class SqliteInMemoryContextFactory : IDbContextFactory<AppDbContext>, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly DbContextOptions<AppDbContext> _options;

    public SqliteInMemoryContextFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        using var context = new AppDbContext(_options);
        context.Database.EnsureCreated();
    }

    public AppDbContext CreateDbContext() => new(_options);

    public void Dispose() => _connection.Dispose();
}
