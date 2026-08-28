using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Book.Query;
using BookCatalog.Application.Responses.Book;
using MediatR;

namespace BookCatalog.Application.Handlers.Book.Query;

public class GetAllBooksQueryHandler : IRequestHandler<GetAllBooksQuery, GetAllBooksResponse>
{
    private readonly IBookRepository _bookRepository;
    private readonly IMapper _mapper;

    public GetAllBooksQueryHandler(IBookRepository bookRepository, IMapper mapper)
    {
        _bookRepository = bookRepository;
        _mapper = mapper;
    }

    public async Task<GetAllBooksResponse> Handle(GetAllBooksQuery request, CancellationToken cancellationToken)
    {
        var books = await _bookRepository.GetAllAsync(request, cancellationToken);
        return new GetAllBooksResponse
        {
            Items = _mapper.Map<IEnumerable<BookResponse>>(books.Items),
            TotalCount = books.TotalCount,
            TotalPages = books.TotalPages,
            Page = books.Page,
            PageSize = books.PageSize
        };
    }
}