using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Interfaces.Repositories;

public interface ILoanRepository : IRepository<Loan, LoanSearchModel>
{
    public Task<Loan?> GetActiveLoanForBookAsync(Guid bookId, CancellationToken cancellationToken);

    public Task<BaseSearchModelPagedResponse<Loan>> GetLoansByUserIdAsync(
        Guid userId,
        LoanSearchModel request,
        CancellationToken cancellationToken);
}
