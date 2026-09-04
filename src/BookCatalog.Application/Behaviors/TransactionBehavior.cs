using BookCatalog.Application.Interfaces.Transactions;
using BookCatalog.Application.Requests;
using MediatR;

namespace BookCatalog.Application.Behaviors;

public class TransactionBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly ITransactionProvider _transactionProvider;

    public TransactionBehavior(ITransactionProvider transactionProvider)
    {
        _transactionProvider = transactionProvider;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken cancellationToken)
    {
        if (request is not ITransactionalCommand)
        {
            return await next();
        }

        return await _transactionProvider.ExecuteAsync(() => next(cancellationToken), cancellationToken);
    }
}
