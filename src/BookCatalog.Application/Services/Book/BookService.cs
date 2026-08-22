using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Domain.Exceptions;

namespace BookCatalog.Application.Services.Book;

public class BookService : IBookService
{
    private readonly IBookRepository _bookRepository;

    public BookService(IBookRepository bookRepository)
    {
        _bookRepository = bookRepository;
    }

    public async Task<Domain.Models.Book> GetOrThrowAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetByIdAsync(id, cancellationToken);
        if (book == null)
            throw new EntityNotFoundException("No book found for a given id.");
        return book;
    }

    public async Task<Domain.Models.Book> GetByIsbnOrThrowAsync(string isbn, CancellationToken cancellationToken = default)
    {
        var book = await _bookRepository.GetBookByIsbnAsync(isbn, cancellationToken);
        if (book == null)
            throw new EntityNotFoundException("No book found for a given isbn.");
        return book;
    }

    public async Task<Domain.Models.Book> CreateAsync(Domain.Models.Book book, CancellationToken cancellationToken = default)
    {
        await _bookRepository.InsertAsync(book, cancellationToken);
        return book;
    }

    public async Task<Domain.Models.Book> UpdateAsync(Domain.Models.Book book, CancellationToken cancellationToken = default)
    {
        await _bookRepository.SaveAsync(cancellationToken);
        return book;
    }

    public async Task DeleteAsync(Domain.Models.Book book, CancellationToken cancellationToken = default)
    {
        await _bookRepository.DeleteEntityAsync(book, cancellationToken);
    }
}