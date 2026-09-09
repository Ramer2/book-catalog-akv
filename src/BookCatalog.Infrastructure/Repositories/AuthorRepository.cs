using System.Linq.Expressions;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class AuthorRepository : IAuthorRepository
{
    private readonly BookCatalogDbContext _dbContext;

    public AuthorRepository(BookCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    private static readonly IReadOnlyDictionary<string, Expression<Func<Author, object>>> SortColumns =
        new Dictionary<string, Expression<Func<Author, object>>>(StringComparer.OrdinalIgnoreCase)
        {
            [nameof(Author.FirstName)] = a => a.FirstName,
            [nameof(Author.LastName)] = a => a.LastName,
            [nameof(Author.CreatedAt)] = a => a.CreatedAt
        };

    public async Task<BaseSearchModelPagedResponse<Author>> GetAllAsync(AuthorSearchModel request,
        CancellationToken cancellationToken)
    {
        var filtered = ApplyFilters(_dbContext.Authors, request);
        var totalCount = await filtered.CountAsync(cancellationToken);

        var ordered = ApplySorting(filtered, request);
        var authors = await ApplyPaging(ordered, request).ToListAsync(cancellationToken);

        return BuildResponse(authors, totalCount, request);
    }

    private static IQueryable<Author> ApplyFilters(IQueryable<Author> query, AuthorSearchModel request)
    {
        if (!string.IsNullOrEmpty(request.FirstName))
            query = query.Where(a => a.FirstName.ToLower().Contains(request.FirstName.ToLower()));

        if (!string.IsNullOrEmpty(request.LastName))
            query = query.Where(a => a.LastName.ToLower().Contains(request.LastName.ToLower()));

        return query;
    }

    private static IOrderedQueryable<Author> ApplySorting(IQueryable<Author> query, AuthorSearchModel request)
    {
        if (!string.IsNullOrWhiteSpace(request.SortBy)
            && SortColumns.TryGetValue(request.SortBy, out var lambda))
        {
            return request.Desc
                ? query.OrderByDescending(lambda).ThenBy(a => a.Id)
                : query.OrderBy(lambda).ThenBy(a => a.Id);
        }

        return query.OrderBy(a => a.Id);
    }

    private static IQueryable<Author> ApplyPaging(IQueryable<Author> query, AuthorSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        return query.Skip((page - 1) * pageSize).Take(pageSize);
    }

    private static BaseSearchModelPagedResponse<Author> BuildResponse(
        IReadOnlyList<Author> items, int totalCount, AuthorSearchModel request)
    {
        var (page, pageSize) = SanitizePaging(request);
        var totalPages = totalCount == 0 ? 0 : (totalCount + pageSize - 1) / pageSize;

        return new BaseSearchModelPagedResponse<Author>
        {
            Items = items,
            TotalCount = totalCount,
            TotalPages = totalPages,
            PageSize = pageSize,
            Page = page
        };
    }

    private static (int Page, int PageSize) SanitizePaging(AuthorSearchModel request)
    {
        var page = request.Page < 1 ? 1 : request.Page;
        var pageSize = request.PageSize < 1 ? 1 : request.PageSize;
        return (page, pageSize);
    }

    public async Task<Author?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        return await _dbContext.Authors.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
    }

    public async Task InsertAsync(Author entity, CancellationToken cancellationToken)
    {
        await _dbContext.Authors.AddAsync(entity, cancellationToken);
        await SaveAsync(cancellationToken);
    }

    public async Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var author = await _dbContext.Authors.FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (author != null)
        {
            _dbContext.Authors.Remove(author);
            await SaveAsync(cancellationToken);
        }
    }

    public Task DeleteEntityAsync(Author entity, CancellationToken cancellationToken)
    {
        _dbContext.Authors.Remove(entity);
        return SaveAsync(cancellationToken);
    }

    public async Task SaveAsync(CancellationToken cancellationToken)
    {
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
