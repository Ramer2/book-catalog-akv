using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;

namespace BookCatalog.Tests.Integration;

public class GracefulShutdownTests : IntegrationTestBase
{
    [Test]
    public void HostOptions_ShutdownTimeout_ShouldBeConfiguredTo30Seconds()
    {
        // Arrange & Act
        var hostOptions = Factory.Services.GetRequiredService<IOptions<HostOptions>>().Value;

        // Assert
        Assert.That(hostOptions.ShutdownTimeout, Is.EqualTo(TimeSpan.FromSeconds(30)));
    }

    [Test]
    public void HostApplicationLifetime_ShouldBeAvailableInServices()
    {
        // Arrange & Act
        var lifetime = Factory.Services.GetService<IHostApplicationLifetime>();

        // Assert
        Assert.That(lifetime, Is.Not.Null);
    }

    [Test]
    public void RequestWithCancelledCancellationToken_ShouldThrowOperationCanceledException()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        cts.Cancel(); // Pre-cancelled token

        // Act & Assert
        Assert.ThrowsAsync<TaskCanceledException>(async () =>
        {
            await Client.GetAsync("/api/books", cts.Token);
        });
    }
}
