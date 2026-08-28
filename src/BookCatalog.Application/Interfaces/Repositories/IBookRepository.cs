using BookCatalog.Domain.Models;
using BookCatalog.Domain.SearchModels;

namespace BookCatalog.Application.Interfaces.Repositories;

public interface IBookRepository : IRepository<Book, BookSearchModel>
{
    public Task<Book?> GetBookByIsbnAsync(string isbn, CancellationToken cancellationToken);
}