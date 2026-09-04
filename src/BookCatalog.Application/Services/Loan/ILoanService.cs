namespace BookCatalog.Application.Services.Loan;

public interface ILoanService
{
    public Task<Domain.Models.Loan> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Domain.Models.Loan> BorrowAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default);
    public Task<Domain.Models.Loan> ReturnAsync(Guid loanId, CancellationToken cancellationToken = default);
    public Task<bool> EnsureBookAvailableAsync(Guid bookId, CancellationToken cancellationToken = default);
}
