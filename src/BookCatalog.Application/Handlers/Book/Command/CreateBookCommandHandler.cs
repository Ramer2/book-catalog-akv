using AutoMapper;
using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Responses.Book;
using BookCatalog.Application.Services.Book;
using MediatR;

namespace BookCatalog.Application.Handlers.Book.Command;

public class CreateBookCommandHandler : IRequestHandler<CreateBookCommand, BookResponse>
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public CreateBookCommandHandler(IMapper mapper, IBookService bookService)
    {
        _mapper = mapper;
        _bookService = bookService;
    }

    public async Task<BookResponse> Handle(CreateBookCommand request, CancellationToken cancellationToken)
    {
        var book = new BookCatalog.Domain.Models.Book(request.Isbn, request.Title, request.Author, request.NumberOfPages, request.PublishDate);
        var createdBook = await _bookService.CreateAsync(book, cancellationToken);
        return _mapper.Map<BookResponse>(createdBook);
    }
}
