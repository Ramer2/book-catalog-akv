using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Responses.Author;
using BookCatalog.Application.Services.Author;
using MediatR;

namespace BookCatalog.Application.Handlers.Author.Command;

public class UpdateAuthorByIdCommandHandler : IRequestHandler<UpdateAuthorByIdCommand, AuthorResponse>
{
    private readonly IAuthorService _authorService;
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public UpdateAuthorByIdCommandHandler(IMapper mapper, IAuthorService authorService, IAuthorRepository authorRepository)
    {
        _mapper = mapper;
        _authorService = authorService;
        _authorRepository = authorRepository;
    }

    public async Task<AuthorResponse> Handle(UpdateAuthorByIdCommand request, CancellationToken cancellationToken)
    {
        var author = await _authorService.GetOrThrowAsync(request.Id, cancellationToken);

        author.FirstName = request.FirstName;
        author.LastName = request.LastName;

        await _authorRepository.SaveAsync(cancellationToken);
        return _mapper.Map<AuthorResponse>(author);
    }
}
