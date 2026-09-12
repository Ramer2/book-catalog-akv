using BookCatalog.Api.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace BookCatalog.Tests.Configuration;

[TestFixture]
public class DatabaseOptionsValidationTests
{
    [Test]
    public void ValidateOnStart_ShouldFail_WhenConnectionStringIsEmpty()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DbConnection", "" },
            { "DbConnection", "" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                var connStr = config.GetConnectionString("DbConnection") ?? config["DbConnection"];
                options.ConnectionString = connStr ?? string.Empty;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var validator = serviceProvider.GetRequiredService<IStartupValidator>();
        Assert.Throws<OptionsValidationException>(() => validator.Validate());
    }

    [Test]
    public void ValidateOnStart_ShouldSucceed_WhenConnectionStringIsProvided()
    {
        // Arrange
        var inMemorySettings = new Dictionary<string, string?>
        {
            { "ConnectionStrings:DbConnection", "Host=localhost;Database=test_db" }
        };

        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(inMemorySettings)
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton(configuration);
        services.AddOptions<DatabaseOptions>()
            .Configure<IConfiguration>((options, config) =>
            {
                var connStr = config.GetConnectionString("DbConnection") ?? config["DbConnection"];
                options.ConnectionString = connStr ?? string.Empty;
            })
            .ValidateDataAnnotations()
            .ValidateOnStart();

        var serviceProvider = services.BuildServiceProvider();

        // Act & Assert
        var validator = serviceProvider.GetRequiredService<IStartupValidator>();
        Assert.DoesNotThrow(() => validator.Validate());
    }
}
