using System.Linq.Expressions;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly BookCatalogDbContext _dbContext;

    public BookRepository(BookCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static readonly IReadOnlyDictionary<string, Expression<Func<Book, object>>> SortColumns =
        new Dictionary<string, Expression<Func<Book, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Book.Title)] = b => b.Title,
            [nameof(Book.Author)] = b => b.Author,
            [nameof(Book.PublishDate)] = b => b.PublishDate ?? DateTime.MinValue
        };

    public async Task<BaseSearchModelPagedResponse<Book>> GetAllAsync(BookSearchModel request,
        CancellationToken cancellationToken)
    {
        var filtered = ApplyFilters(_dbContext.Books, request);
        var totalCount = await filtered.CountAsync(cancellationToken);

        var ordered = ApplySorting(filtered, request);
        var books = await ApplyPaging(ordered, request).ToListAsync(cancellationToken);

        return BuildResponse(books, totalCount, request);
    }

    private static IQueryable<Book> ApplyFilters(IQueryable<Book> query, BookSearchModel request)
    {
        if (!string.IsNullOrEmpty(request.Title))
            query = query.Where(b => b.Title.ToLower().Contains(request.Title.ToLower()));

        if (!string.IsNullOrEmpty(request.Author))
            query = query.Where(b => b.Author.ToLower().Contains(request.Author.ToLower()));

        if (!string.IsNullOrEmpty(request.Isbn))
            query = query.Where(b => b.Isbn.ToLower().Contains(request.Isbn.ToLower()));

        return query;
    }

    private static IOrderedQueryable<Book> ApplySorting(IQueryable<Book> query, BookSearchModel request)
    {
        if (!string.IsNullOrWhiteSpace(request.SortBy)
            && SortColumns.TryGetValue(request.SortBy, out var lambda))
        {
            return request.Desc
                ? query.OrderByDescending(lambda).ThenBy(b => b.Id)
                : query.OrderBy(lambda).ThenBy(b => b.Id);
        }

        // deterministic default so Skip/Take pages stay stable when SortBy is null or unknown
        return query.OrderBy(b => b.Id);
    }

    private static IQueryable<Book> ApplyPaging(IQueryable<Book> query, BookSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    private static BaseSearchModelPagedResponse<Book> BuildResponse(
        IReadOnlyList<Book> items, int totalCount, BookSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        var totalPages = totalCount == 0 ? 0 : (totalCount + pageSize - 1) / pageSize;

        return new BaseSearchModelPagedResponse<Book>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };
    }

    private static (int Page, int PageSize) SanitizePaging(BookSearchModel request)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 1 : request.PageSize;
        return (page, pageSize);
    }

    public async Task<Book?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
    }

    public async Task<Book?> GetBookByIsbnAsync(string isbn, CancellationToken cancellationToken)
    {
        return await _dbContext.Books.FirstOrDefaultAsync(b => b.Isbn == isbn, cancellationToken);
    }

    public async Task InsertAsync(Book entity, CancellationToken cancellationToken)
    {
        await _dbContext.Books.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var book = await _dbContext.Books.FirstOrDefaultAsync(b => b.Id == id, cancellationToken);
        if (book != null)
        {
            _dbContext.Books.Remove(book);
            await SaveAsync(cancellationToken);
        }
    }

    public Task DeleteEntityAsync(Book entity, CancellationToken cancellationToken)
    {
        _dbContext.Books.Remove(entity);
        return SaveAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}