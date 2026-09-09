namespace BookCatalog.Application.Services.Author;

public interface IAuthorService
{
    public Task<Domain.Models.Author> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default);
}
