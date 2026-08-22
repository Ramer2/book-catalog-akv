using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Infrastructure.Repositories;

public class BookRepository : IBookRepository
{
    private readonly BookCatalogDbContext _dbContext;

    public BookRepository(BookCatalogDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<List<Book>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await _dbContext.Books.ToListAsync(cancellationToken);
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