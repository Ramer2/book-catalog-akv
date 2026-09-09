using BookCatalog.Application.Interfaces.Persistence;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Exceptions;

namespace BookCatalog.Application.Services.Loan;

public class LoanService : ILoanService
{
    public const string ActiveLoanIndexName = "UX_Loan_BookId_Active";

    private readonly ILoanRepository _loanRepository;
    private readonly IDbExceptionInterpreter _dbExceptionInterpreter;

    public LoanService(
        ILoanRepository loanRepository,
        IDbExceptionInterpreter dbExceptionInterpreter)
    {
        _loanRepository = loanRepository;
        _dbExceptionInterpreter = dbExceptionInterpreter;
    }

    public async Task<Domain.Models.Loan> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var loan = await _loanRepository.GetByIdAsync(id, cancellationToken);
        if (loan == null)
            throw new EntityNotFoundException("No loan found for a given id.");
        return loan;
    }

    public async Task<Domain.Models.Loan> BorrowAsync(Guid bookId, Guid userId,
        CancellationToken cancellationToken = default)
    {
        var loan = new Domain.Models.Loan(bookId, userId);
        try
        {
            await _loanRepository.InsertAsync(loan, cancellationToken);
        }
        catch (Exception ex) when (_dbExceptionInterpreter.IsUniqueViolation(ex, ActiveLoanIndexName))
        {
            // A concurrent borrow slipped past the availability check but the
            // database rejected the duplicate active loan. Translate into a
            // domain-friendly exception; the surrounding transaction will
            // roll back and the API filter will surface it as HTTP 409.
            throw new BookAlreadyBorrowedException(
                "Book is currently borrowed and cannot be lent out.", ex);
        }

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