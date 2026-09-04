using AutoMapper;
using BookCatalog.Application.Handlers.Author.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Author;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class UpdateAuthorByIdCommandHandlerTests
{
    private Mock<IAuthorRepository> _authorRepository = null!;
    private Mock<IAuthorService> _authorService = null!;
    private IMapper _mapper = null!;
    private UpdateAuthorByIdCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authorRepository = new Mock<IAuthorRepository>(MockBehavior.Strict);
        _authorService = new Mock<IAuthorService>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new UpdateAuthorByIdCommandHandler(_mapper, _authorService.Object, _authorRepository.Object);
    }

    [Test]
    public async Task Handle_Should_MutateExistingAuthorAndReturnMappedResponse_WhenAuthorExists()
    {
        var existing = AuthorFaker.Author(firstName: "OldFirst", lastName: "OldLast");

        _authorService
            .Setup(s => s.GetOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _authorRepository
            .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = AuthorFaker.UpdateCommand(id: existing.Id, firstName: "NewFirst", lastName: "NewLast");

        var response = await _sut.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(existing.FirstName, Is.EqualTo(command.FirstName));
            Assert.That(existing.LastName, Is.EqualTo(command.LastName));
            Assert.That(existing.Id, Is.EqualTo(command.Id));
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(command.Id));
            Assert.That(response.FirstName, Is.EqualTo(command.FirstName));
            Assert.That(response.LastName, Is.EqualTo(command.LastName));
        });

        _authorRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_AuthorDoesNotExist()
    {
        var command = AuthorFaker.UpdateCommand();
        _authorService
            .Setup(s => s.GetOrThrowAsync(command.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("No author found for a given id."));

        Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(command, CancellationToken.None));
    }
}
