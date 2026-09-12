using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace BookCatalog.Tests.Integration;

public class HealthChecksIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task GetLiveness_ShouldReturn200OK_WithHealthyStatus()
    {
        // Act
        var response = await Client.GetAsync("/health/live");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("status").GetString(), Is.EqualTo("Healthy"));
    }

    [Test]
    public async Task GetReadiness_ShouldReturn200OK_WithDatabaseCheckHealthy()
    {
        // Act
        var response = await Client.GetAsync("/health/ready");

        // Assert
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.OK));

        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.That(json.GetProperty("status").GetString(), Is.EqualTo("Healthy"));

        var entries = json.GetProperty("entries");
        Assert.That(entries.GetArrayLength(), Is.GreaterThan(0));

        var dbCheck = entries.EnumerateArray().FirstOrDefault(e => e.GetProperty("name").GetString() == "database");
        Assert.That(dbCheck.GetProperty("status").GetString(), Is.EqualTo("Healthy"));
    }
}
