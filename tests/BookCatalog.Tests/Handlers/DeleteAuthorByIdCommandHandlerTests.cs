using BookCatalog.Application.Handlers.Author.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Author.Command;
using BookCatalog.Application.Services.Author;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class DeleteAuthorByIdCommandHandlerTests
{
    private Mock<IAuthorRepository> _authorRepository = null!;
    private Mock<IAuthorService> _authorService = null!;
    private DeleteAuthorByIdCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authorRepository = new Mock<IAuthorRepository>(MockBehavior.Strict);
        _authorService = new Mock<IAuthorService>(MockBehavior.Strict);
        _sut = new DeleteAuthorByIdCommandHandler(_authorService.Object, _authorRepository.Object);
    }

    [Test]
    public async Task Handle_Should_DeleteAuthor_When_AuthorExists()
    {
        var existing = AuthorFaker.Author();
        _authorService
            .Setup(s => s.GetOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _authorRepository
            .Setup(r => r.DeleteEntityAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = new DeleteAuthorByIdCommand { Id = existing.Id };

        await _sut.Handle(command, CancellationToken.None);

        _authorRepository.Verify(
            r => r.DeleteEntityAsync(existing, It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_AuthorDoesNotExist()
    {
        var command = new DeleteAuthorByIdCommand { Id = Guid.NewGuid() };
        _authorService
            .Setup(s => s.GetOrThrowAsync(command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("No author found for a given id."));

        Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(command, CancellationToken.None));
    }
}
