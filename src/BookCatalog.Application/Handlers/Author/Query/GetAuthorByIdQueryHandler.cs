using AutoMapper;
using BookCatalog.Application.Requests.Author.Query;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Application.Services.Author;
using MediatR;

namespace BookCatalog.Application.Handlers.Author.Query;

public class GetAuthorByIdQueryHandler : IRequestHandler<GetAuthorByIdQuery, AuthorResponse>
{
    private readonly IAuthorService _authorService;
    private readonly IMapper _mapper;

    public GetAuthorByIdQueryHandler(IMapper mapper, IAuthorService authorService)
    {
        _mapper = mapper;
        _authorService = authorService;
    }

    public async Task<AuthorResponse> Handle(GetAuthorByIdQuery request, CancellationToken cancellationToken)
    {
        var author = await _authorService.GetOrThrowAsync(request.Id, cancellationToken);
        return _mapper.Map<AuthorResponse>(author);
    }
}
