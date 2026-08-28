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

    public async Task<BaseSearchModelPagedResponse<Book>> GetAllAsync(BookSearchModel request,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Books.AsQueryable();

        // filtering
        if (!string.IsNullOrEmpty(request.Title))
            query = query.Where(p => p.Title.ToLower().Contains(request.Title.ToLower()));

        if (!string.IsNullOrEmpty(request.Author))
            query = query.Where(p => p.Author.ToLower().Contains(request.Author.ToLower()));

        if (!string.IsNullOrEmpty(request.Isbn))
            query = query.Where(p => p.Isbn.ToLower().Contains(request.Isbn.ToLower()));
        
        var totalCount = await query.CountAsync(cancellationToken);

        // ordering
        if (!string.IsNullOrWhiteSpace(request.SortBy))
        {
            var field = request.SortBy.ToLower();
            // object, because different types for different fields
            Expression<Func<Book, object>>? lambda = null;
            // if instead of a switch because doesn't compile nameof(...).ToLower() in switch - needs constant data
            if (field == nameof(Book.Title).ToLower())
                lambda = p => p.Title;
            else if (field == nameof(Book.Author).ToLower())
                lambda = p => p.Author;
            else if (field == nameof(Book.PublishDate).ToLower())
                lambda = p => p.PublishDate ?? new DateTime();

            if (lambda != null)
            {
                if (request.Desc)
                    query = query.OrderByDescending(lambda);
                else
                    query = query.OrderBy(lambda);
            }
        }

        // pagination
        var products = await query
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync(cancellationToken);

        return new BaseSearchModelPagedResponse<Book>
        {
            Items = products,
            TotalCount = totalCount,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize),
            PageSize = request.PageSize,
            Page = request.Page
        };
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