using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Services.Book;
using MediatR;

namespace BookCatalog.Application.Handlers.Book.Command;

public class DeleteBookByIdCommandHandler : IRequestHandler<DeleteBookByIdCommand>
{
    private readonly IBookService _bookService;

    public DeleteBookByIdCommandHandler(IBookService bookService)
    {
        _bookService = bookService;
    }

    public async Task Handle(DeleteBookByIdCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetOrThrowAsync(request.Id, cancellationToken);
        await _bookService.DeleteAsync(book, cancellationToken);
    }
}
