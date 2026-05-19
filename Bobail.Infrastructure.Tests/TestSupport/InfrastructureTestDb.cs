using Bobail.Infrastructure.Persistence;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Bobail.Infrastructure.Tests.TestSupport;

public sealed class InfrastructureTestDb : IDisposable
{
    private readonly SqliteConnection _connection;

    private InfrastructureTestDb()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<GameDbContext>()
            .UseSqlite(_connection)
            .Options;

        Context = new GameDbContext(options);
        Context.Database.EnsureCreated();
    }

    public GameDbContext Context { get; }

    public static InfrastructureTestDb Create()
    {
        return new InfrastructureTestDb();
    }

    public void Dispose()
    {
        Context.Dispose();
        _connection.Dispose();
    }
}
