using BookCatalog.Application.Interfaces.Transactions;
using BookCatalog.Domain.Models;
using BookCatalog.Infrastructure;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace BookCatalog.Tests.Integration;

public class TransactionResilienceIntegrationTests : IntegrationTestBase
{
    [Test]
    public async Task ExecuteAsync_ShouldCommitChanges_WhenActionSucceeds()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var transactionProvider = scope.ServiceProvider.GetRequiredService<ITransactionProvider>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();

        var user = new User(
            email: "resilience.user@test.com",
            phoneNumber: "+1234567890",
            firstName: "Resilience",
            lastName: "User",
            birthDate: new DateOnly(1990, 1, 1));

        // Act
        var result = await transactionProvider.ExecuteAsync(async () =>
        {
            await dbContext.Users.AddAsync(user);
            return user.Id;
        });

        // Assert
        Assert.That(result, Is.EqualTo(user.Id));

        using var verifyScope = Factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();
        var savedUser = await verifyDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.That(savedUser, Is.Not.Null);
        Assert.That(savedUser!.Email, Is.EqualTo("resilience.user@test.com"));
    }

    [Test]
    public async Task ExecuteAsync_ShouldRollbackChanges_WhenActionThrowsException()
    {
        // Arrange
        using var scope = Factory.Services.CreateScope();
        var transactionProvider = scope.ServiceProvider.GetRequiredService<ITransactionProvider>();
        var dbContext = scope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();

        var user = new User(
            email: "rollback.user@test.com",
            phoneNumber: "+1234567899",
            firstName: "Rollback",
            lastName: "User",
            birthDate: new DateOnly(1990, 1, 1));

        // Act & Assert
        Assert.ThrowsAsync<InvalidOperationException>(async () =>
        {
            await transactionProvider.ExecuteAsync<Guid>(async () =>
            {
                await dbContext.Users.AddAsync(user);
                await dbContext.SaveChangesAsync();
                throw new InvalidOperationException("Simulated failure after entity add");
            });
        });

        // Verify entity was rolled back and is not present in DB
        using var verifyScope = Factory.Services.CreateScope();
        var verifyDbContext = verifyScope.ServiceProvider.GetRequiredService<BookCatalogDbContext>();
        var nonExistentUser = await verifyDbContext.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        Assert.That(nonExistentUser, Is.Null);
    }
}
