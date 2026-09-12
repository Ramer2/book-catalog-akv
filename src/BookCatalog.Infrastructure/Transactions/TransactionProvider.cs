using BookCatalog.Application.Interfaces.Transactions;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Transactions;

public class TransactionProvider : ITransactionProvider
{
    private readonly BookCatalogDbContext _context;

    public TransactionProvider(BookCatalogDbContext context)
    {
        _context = context;
    }

    public async Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default)
    {
        var strategy = _context.Database.CreateExecutionStrategy();

        return await strategy.ExecuteAsync(async () =>
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            try
            {
                var result = await action();

                await _context.SaveChangesAsync(cancellationToken);
                await transaction.CommitAsync(cancellationToken);

                return result;
            }
            catch
            {
                await transaction.RollbackAsync(cancellationToken);
                throw;
            }
        });
    }
}