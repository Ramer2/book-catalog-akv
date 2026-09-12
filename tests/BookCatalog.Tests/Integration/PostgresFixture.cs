using Testcontainers.PostgreSql;

namespace BookCatalog.Tests.Integration;

public class PostgresFixture
{
    private readonly PostgreSqlContainer _container;

    public PostgresFixture()
    {
        _container = new PostgreSqlBuilder()
            .WithImage("postgres:latest")
            .WithDatabase("postgres_books")
            .WithUsername("postgres")
            .WithPassword("P@ssw0rd")
            .Build();
    }

    public string ConnectionString => _container.GetConnectionString();

    public async Task InitializeAsync()
    {
        await _container.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}
