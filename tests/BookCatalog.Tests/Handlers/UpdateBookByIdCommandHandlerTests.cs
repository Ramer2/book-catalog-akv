using AutoMapper;
using BookCatalog.Application.Handlers.Book.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Book;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class UpdateBookByIdCommandHandlerTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private IMapper _mapper = null!;
    private BookService _bookService = null!;
    private UpdateBookByIdCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _bookService = new BookService(_bookRepository.Object);
        _sut = new UpdateBookByIdCommandHandler(_mapper, _bookService);
    }

    [Test]
    public async Task Handle_Should_MutateExistingBookAndReturnMappedResponse_WhenBookExists()
    {
        var existing = BookFaker.Book(
            isbn: "0000000001",
            title: "old title",
            author: "old author",
            numberOfPages: 10,
            publishDate: new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        _bookRepository
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);
        _bookRepository
            .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var command = BookFaker.UpdateCommand(id: existing.Id);

        var response = await _sut.Handle(command, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(existing.Isbn, Is.EqualTo(command.Isbn),
                "Handler must overwrite Book.Isbn from the command");
            Assert.That(existing.Title, Is.EqualTo(command.Title),
                "Handler must overwrite Book.Title from the command");
            Assert.That(existing.Author, Is.EqualTo(command.Author),
                "Handler must overwrite Book.Author from the command");
            Assert.That(existing.NumberOfPages, Is.EqualTo(command.NumberOfPages),
                "Handler must overwrite Book.NumberOfPages from the command");
            Assert.That(existing.PublishDate, Is.EqualTo(command.PublishDate),
                "Handler must overwrite Book.PublishDate from the command");
            Assert.That(existing.Id, Is.EqualTo(command.Id),
                "Handler must not alter Book.Id during update");
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(command.Id),
                "BookResponse.Id must reflect the updated book's Id");
            Assert.That(response.Isbn, Is.EqualTo(command.Isbn),
                "BookResponse.Isbn must reflect the newly assigned ISBN");
            Assert.That(response.Title, Is.EqualTo(command.Title),
                "BookResponse.Title must reflect the newly assigned title");
        });

        _bookRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "UpdateBookByIdCommandHandler must flush changes via IBookRepository.SaveAsync exactly once");
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_BookDoesNotExist()
    {
        var command = BookFaker.UpdateCommand();
        _bookRepository
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(command, CancellationToken.None),
            "UpdateBookByIdCommandHandler must throw EntityNotFoundException when the target book does not exist");

        Assert.That(ex!.Message, Is.EqualTo("No book found for a given id."),
            "EntityNotFoundException should carry the id-based lookup message from BookService.GetOrThrowAsync");

        _bookRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "UpdateBookByIdCommandHandler must NOT call SaveAsync when the target book cannot be found");
    }
}
