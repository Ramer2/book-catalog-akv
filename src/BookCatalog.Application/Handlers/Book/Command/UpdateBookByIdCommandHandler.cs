using AutoMapper;
using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Responses.Book;
using BookCatalog.Application.Services.Book;
using MediatR;

namespace BookCatalog.Application.Handlers.Book.Command;

public class UpdateBookByIdCommandHandler : IRequestHandler<UpdateBookByIdCommand, BookResponse>
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public UpdateBookByIdCommandHandler(IMapper mapper, IBookService bookService)
    {
        _mapper = mapper;
        _bookService = bookService;
    }

    public async Task<BookResponse> Handle(UpdateBookByIdCommand request, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetOrThrowAsync(request.Id, cancellationToken);

        book.Isbn = request.Isbn;
        book.Title = request.Title;
        book.Author = request.Author;
        book.NumberOfPages = request.NumberOfPages;
        book.PublishDate = request.PublishDate;

        var updatedBook = await _bookService.UpdateAsync(book, cancellationToken);
        return _mapper.Map<BookResponse>(updatedBook);
    }
}
