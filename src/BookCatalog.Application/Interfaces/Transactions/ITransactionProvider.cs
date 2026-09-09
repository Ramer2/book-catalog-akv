namespace BookCatalog.Application.Interfaces.Transactions;

public interface ITransactionProvider
{
    public Task<T> ExecuteAsync<T>(Func<Task<T>> action, CancellationToken cancellationToken = default);
}