using BookCatalog.Application.Handlers.Book.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Services.Book;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class DeleteBookByIdCommandHandlerTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private BookService _bookService = null!;
    private DeleteBookByIdCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _bookService = new BookService(_bookRepository.Object);
        _sut = new DeleteBookByIdCommandHandler(_bookService);
    }

    [Test]
    public async Task Handle_Should_DeleteBook_WhenBookExists()
    {
        var existing = BookFaker.Book();
        _bookRepository
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _bookRepository
            .Setup(r => r.DeleteEntityAsync(existing, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.Handle(new DeleteBookByIdCommand { Id = existing.Id }, CancellationToken.None);

        _bookRepository.Verify(
            r => r.DeleteEntityAsync(existing, It.IsAny<CancellationToken>()),
            Times.Once,
            "DeleteBookByIdCommandHandler must delete the exact instance returned by GetByIdAsync");
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_BookDoesNotExist()
    {
        var command = new DeleteBookByIdCommand { Id = Guid.NewGuid() };
        _bookRepository
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(command, CancellationToken.None),
            "DeleteBookByIdCommandHandler must throw EntityNotFoundException when the target book does not exist");

        Assert.That(ex!.Message, Is.EqualTo("No book found for a given id."),
            "EntityNotFoundException should carry the id-based lookup message");

        _bookRepository.Verify(
            r => r.DeleteEntityAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "DeleteBookByIdCommandHandler must NOT call DeleteEntityAsync when the target book cannot be found");
    }
}
