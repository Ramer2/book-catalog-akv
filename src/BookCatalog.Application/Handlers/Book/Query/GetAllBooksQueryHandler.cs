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
        var books = await _bookRepository.GetAllAsync(cancellationToken);
        return new GetAllBooksResponse
        {
            Books = _mapper.Map<List<BookResponse>>(books)
        };
    }
}