using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Exceptions;

namespace BookCatalog.Application.Services.Loan;

public class LoanService : ILoanService
{
    private readonly ILoanRepository _loanRepository;

    public LoanService(ILoanRepository loanRepository)
    {
        _loanRepository = loanRepository;
    }

    public async Task<Domain.Models.Loan> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await _loanRepository.GetByIdAsync(id, cancellationToken);
        if (loan == null)
            throw new EntityNotFoundException("No loan found for a given id.");
        return loan;
    }

    public async Task<Domain.Models.Loan> BorrowAsync(Guid bookId, Guid userId, CancellationToken cancellationToken = default)
    {
        var loan = new Domain.Models.Loan(bookId, userId);
        await _loanRepository.InsertAsync(loan, cancellationToken);
        return loan;
    }

    public async Task<Domain.Models.Loan> ReturnAsync(Guid loanId, CancellationToken cancellationToken = default)
    {
        var loan = await GetOrThrowAsync(loanId, cancellationToken);
        if (loan.ReturnedAt.HasValue)
            return loan;

        loan.ReturnedAt = DateTime.UtcNow;
        await _loanRepository.SaveAsync(cancellationToken);
        return loan;
    }

    public async Task<bool> EnsureBookAvailableAsync(Guid bookId, CancellationToken cancellationToken = default)
    {
        var activeLoan = await _loanRepository.GetActiveLoanForBookAsync(bookId, cancellationToken);
        return activeLoan is null;
    }
}
