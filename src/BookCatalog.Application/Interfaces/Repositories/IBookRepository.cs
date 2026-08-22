using BookCatalog.Domain.Models;

namespace BookCatalog.Application.Interfaces.Repositories;

public interface IBookRepository : IRepository<Book>
{
    public Task<Book?> GetBookByIsbnAsync(string isbn, CancellationToken cancellationToken);
}