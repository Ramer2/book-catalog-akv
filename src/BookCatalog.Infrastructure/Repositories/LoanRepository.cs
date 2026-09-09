using System.Linq.Expressions;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class LoanRepository : ILoanRepository
{
    private readonly BookCatalogDbContext _dbContext;

    public LoanRepository(BookCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static readonly IReadOnlyDictionary<string, Expression<Func<Loan, object>>> SortColumns =
        new Dictionary<string, Expression<Func<Loan, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Loan.BorrowedAt)] = l => l.BorrowedAt,
            [nameof(Loan.ReturnedAt)] = l => l.ReturnedAt ?? DateTime.MaxValue
        };

    public async Task<BaseSearchModelPagedResponse<Loan>> GetAllAsync(LoanSearchModel request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .AsQueryable();

        var filtered = ApplyFilters(query, request);
        var totalCount = await filtered.CountAsync(cancellationToken);

        var ordered = ApplySorting(filtered, request);
        var loans = await ApplyPaging(ordered, request).ToListAsync(cancellationToken);

        return BuildResponse(loans, totalCount, request);
    }

    public async Task<BaseSearchModelPagedResponse<Loan>> GetLoansByUserIdAsync(
        Guid userId,
        LoanSearchModel request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .Where(l => l.UserId == userId);

        var filtered = ApplyFilters(query, request);
        var totalCount = await filtered.CountAsync(cancellationToken);

        var ordered = ApplySorting(filtered, request);
        var loans = await ApplyPaging(ordered, request).ToListAsync(cancellationToken);

        return BuildResponse(loans, totalCount, request);
    }

    public async Task<Loan?> GetActiveLoanForBookAsync(Guid bookId, CancellationToken cancellationToken)
    {
        return await _dbContext.Loans
            .FirstOrDefaultAsync(l => l.BookId == bookId && l.ReturnedAt == null, cancellationToken);
    }

    private static IQueryable<Loan> ApplyFilters(IQueryable<Loan> query, LoanSearchModel request)
    {
        if (request.UserId.HasValue)
            query = query.Where(l => l.UserId == request.UserId.Value);

        if (request.BookId.HasValue)
            query = query.Where(l => l.BookId == request.BookId.Value);

        if (request.IsReturned.HasValue)
        {
            query = request.IsReturned.Value
                ? query.Where(l => l.ReturnedAt != null)
                : query.Where(l => l.ReturnedAt == null);
        }

        return query;
    }

    private static IOrderedQueryable<Loan> ApplySorting(IQueryable<Loan> query, LoanSearchModel request)
    {
        if (!string.IsNullOrWhiteSpace(request.SortBy)
            && SortColumns.TryGetValue(request.SortBy, out var lambda))
        {
            return request.Desc
                ? query.OrderByDescending(lambda).ThenBy(l => l.Id)
                : query.OrderBy(lambda).ThenBy(l => l.Id);
        }

        // default: most recent borrows first
        return query.OrderByDescending(l => l.BorrowedAt).ThenBy(l => l.Id);
    }

    private static IQueryable<Loan> ApplyPaging(IQueryable<Loan> query, LoanSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    private static BaseSearchModelPagedResponse<Loan> BuildResponse(
        IReadOnlyList<Loan> items, int totalCount, LoanSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        var totalPages = totalCount == 0 ? 0 : (totalCount + pageSize - 1) / pageSize;

        return new BaseSearchModelPagedResponse<Loan>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };
    }

    private static (int Page, int PageSize) SanitizePaging(LoanSearchModel request)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 1 : request.PageSize;
        return (page, pageSize);
    }

    public async Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Loans
            .Include(l => l.Book)
            .Include(l => l.User)
            .FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
    }

    public async Task InsertAsync(Loan entity, CancellationToken cancellationToken)
    {
        await _dbContext.Loans.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var loan = await _dbContext.Loans.FirstOrDefaultAsync(l => l.Id == id, cancellationToken);
        if (loan != null)
        {
            _dbContext.Loans.Remove(loan);
            await SaveAsync(cancellationToken);
        }
    }

    public Task DeleteEntityAsync(Loan entity, CancellationToken cancellationToken)
    {
        _dbContext.Loans.Remove(entity);
        return SaveAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
