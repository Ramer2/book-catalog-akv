using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;

namespace BookCatalog.Application.Interfaces;

// generic repo interface
public interface IRepository<T, TSearchModel>
{
    public Task<BaseSearchModelPagedResponse<T>> GetAllAsync(TSearchModel request, CancellationToken cancellationToken);
    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task InsertAsync(T entity, CancellationToken cancellationToken);
    public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken);
    public Task DeleteEntityAsync(T entity, CancellationToken cancellationToken);
    public Task SaveAsync(CancellationToken cancellationToken);
}