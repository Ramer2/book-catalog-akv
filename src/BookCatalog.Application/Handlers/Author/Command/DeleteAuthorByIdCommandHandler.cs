using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Services.Author;
using MediatR;

namespace BookCatalog.Application.Handlers.Author.Command;

public class DeleteAuthorByIdCommandHandler : IRequestHandler<DeleteAuthorByIdCommand>
{
    private readonly IAuthorService _authorService;
    private readonly IAuthorRepository _authorRepository;

    public DeleteAuthorByIdCommandHandler(IAuthorService authorService, IAuthorRepository authorRepository)
    {
        _authorService = authorService;
        _authorRepository = authorRepository;
    }

    public async Task Handle(DeleteAuthorByIdCommand request, CancellationToken cancellationToken)
    {
        var author = await _authorService.GetOrThrowAsync(request.Id, cancellationToken);
        await _authorRepository.DeleteEntityAsync(author, cancellationToken);
    }
}
