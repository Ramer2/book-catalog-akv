namespace BookCatalog.Tests.Integration;

[TestFixture]
[Category("Integration")]
public abstract class IntegrationTestBase
{
    protected HttpClient Client { get; private set; } = null!;
    protected CustomWebApplicationFactory Factory => IntegrationTestAssemblySetup.Factory;

    [SetUp]
    public async Task BaseSetUp()
    {
        await Factory.ResetDatabaseAsync();
        Client = Factory.CreateClient();
    }

    [TearDown]
    public void BaseTearDown()
    {
        Client?.Dispose();
    }
}
