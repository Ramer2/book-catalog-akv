using BookCatalog.Application.Interfaces.Repositories;

namespace BookCatalog.Application.Services.Isbn;

public class IsbnService : IIsbnService
{
    private readonly IBookRepository _bookRepository;

    public IsbnService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<bool> EnsureIsbnUniqueAsync(
        string isbn,
        Guid? excludeBookId = null,
        CancellationToken cancellationToken = default)
    {
        var existing = await _bookRepository.GetBookByIsbnAsync(isbn, cancellationToken);
        if (existing is null)
            return true;
        return excludeBookId.HasValue && existing.Id == excludeBookId.Value;
    }
}
