namespace BookCatalog.Tests.Integration;

[SetUpFixture]
public class IntegrationTestAssemblySetup
{
    public static PostgresFixture PostgresFixture { get; private set; } = null!;
    public static CustomWebApplicationFactory Factory { get; private set; } = null!;

    [OneTimeSetUp]
    public async Task RunBeforeAnyTests()
    {
        PostgresFixture = new PostgresFixture();
        await PostgresFixture.InitializeAsync();

        Factory = new CustomWebApplicationFactory(PostgresFixture.ConnectionString);
        await Factory.EnsureDatabaseMigratedAsync();
    }

    [OneTimeTearDown]
    public async Task RunAfterAnyTests()
    {
        if (Factory != null)
            await Factory.DisposeAsync();

        if (PostgresFixture != null)
            await PostgresFixture.DisposeAsync();
    }
}
