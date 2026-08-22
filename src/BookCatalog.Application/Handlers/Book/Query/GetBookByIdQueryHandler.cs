using AutoMapper;
using BookCatalog.Application.Requests.Book.Query;
using BookCatalog.Application.Responses.Book.Query;
using BookCatalog.Application.Services.Book;
using MediatR;

namespace BookCatalog.Application.Handlers.Book.Query;

public class GetBookByIdQueryHandler : IRequestHandler<GetBookByIdQuery, BookResponse>
{
    private readonly IBookService _bookService;
    private readonly IMapper _mapper;

    public GetBookByIdQueryHandler(IMapper mapper, IBookService bookService)
    {
        _mapper = mapper;
        _bookService = bookService;
    }

    public async Task<BookResponse> Handle(GetBookByIdQuery request, CancellationToken cancellationToken)
    {
        var book = await _bookService.GetOrThrowAsync(request.Id, cancellationToken);
        return _mapper.Map<BookResponse>(book);
    }
}