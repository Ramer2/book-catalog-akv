namespace BookCatalog.Application.Services.Book;

public interface IBookService
{
    public Task<Domain.Models.Book> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default);
    public Task<Domain.Models.Book> GetByIsbnOrThrowAsync(string isbn, CancellationToken cancellationToken = default);
    public Task<Domain.Models.Book> CreateAsync(Domain.Models.Book book, CancellationToken cancellationToken = default);
    public Task<Domain.Models.Book> UpdateAsync(Domain.Models.Book book, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Domain.Models.Book book, CancellationToken cancellationToken = default);
}