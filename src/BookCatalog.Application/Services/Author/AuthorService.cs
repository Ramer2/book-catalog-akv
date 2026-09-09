using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Exceptions;

namespace BookCatalog.Application.Services.Author;

public class AuthorService : IAuthorService
{
    private readonly IAuthorRepository _authorRepository;

    public AuthorService(IAuthorRepository authorRepository)
    {
        _authorRepository = authorRepository;
    }

    public async Task<Domain.Models.Author> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var author = await _authorRepository.GetByIdAsync(id, cancellationToken);
        if (author == null)
            throw new EntityNotFoundException("No author found for a given id.");
        return author;
    }
}
