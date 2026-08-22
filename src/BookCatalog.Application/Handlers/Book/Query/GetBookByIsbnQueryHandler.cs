using AutoMapper;
using BookCatalog.Application.Requests.Book.Query;
using BookCatalog.Application.Responses.Book.Query;
using BookCatalog.Application.Services.Book;
using MediatR;

namespace BookCatalog.Application.Handlers.Book.Query;

public class GetBookByIsbnQueryHandler : IRequestHandler<GetBookByIsbnQuery, BookResponse>
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public GetBookByIsbnQueryHandler(IMapper mapper, IBookService bookService)
    {
        _mapper = mapper;
        _bookService = bookService;
    }

    public async Task<BookResponse> Handle(GetBookByIsbnQuery request, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetByIsbnOrThrowAsync(request.Isbn, cancellationToken);
        return _mapper.Map<BookResponse>(book);
    }
}
