using AutoMapper;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Responses.Author;
using MediatR;

namespace BookCatalog.Application.Handlers.Author.Command;

public class CreateAuthorCommandHandler : IRequestHandler<CreateAuthorCommand, AuthorResponse>
{
    private readonly IAuthorRepository _authorRepository;
    private readonly IMapper _mapper;

    public CreateAuthorCommandHandler(IMapper mapper, IAuthorRepository authorRepository)
    {
        _mapper = mapper;
        _authorRepository = authorRepository;
    }

    public async Task<AuthorResponse> Handle(CreateAuthorCommand request, CancellationToken cancellationToken)
    {
        var author = new Domain.Models.Author(request.FirstName, request.LastName);
        await _authorRepository.InsertAsync(author, cancellationToken);
        return _mapper.Map<AuthorResponse>(author);
    }
}
