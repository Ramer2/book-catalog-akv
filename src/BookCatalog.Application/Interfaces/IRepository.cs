using BookCatalog.Domain.Models;

namespace BookCatalog.Application.Interfaces;

// generic repo interface
public interface IRepository<T>
{
    public Task<List<Book>> GetAllAsync(CancellationToken cancellationToken);
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task InsertAsync(T entity, CancellationToken cancellationToken);
    public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task DeleteEntityAsync(T entity, CancellationToken cancellationToken);
    public Task SaveAsync(CancellationToken cancellationToken);
}