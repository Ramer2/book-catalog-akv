namespace BookCatalog.Application.Services.Isbn;

public interface IIsbnService
{
    public Task<bool> EnsureIsbnUniqueAsync(
        string isbn,
        Guid? excludeBookId = null,
        CancellationToken cancellationToken = default);
}
