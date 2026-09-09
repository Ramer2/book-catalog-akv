using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Author.Query;
using BookCatalog.Application.Responses.Author;
using MediatR;

namespace BookCatalog.Application.Handlers.Author.Query;

public class GetAllAuthorsQueryHandler : IRequestHandler<GetAllAuthorsQuery, GetAllAuthorsResponse>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public GetAllAuthorsQueryHandler(IAuthorRepository authorRepository, IMapper mapper)
    {
        _authorRepository = authorRepository;
        _mapper = mapper;
    }

    public async Task<GetAllAuthorsResponse> Handle(GetAllAuthorsQuery request, CancellationToken cancellationToken)
    {
        var authors = await _authorRepository.GetAllAsync(request, cancellationToken);
        return new GetAllAuthorsResponse
        {
            Items = _mapper.Map<IEnumerable<AuthorResponse>>(authors.Items),
            TotalCount = authors.TotalCount,
            TotalPages = authors.TotalPages,
            Page = authors.Page,
            PageSize = authors.PageSize
        };
    }
}
