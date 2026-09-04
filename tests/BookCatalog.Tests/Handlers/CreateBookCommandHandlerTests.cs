using AutoMapper;
using BookCatalog.Application.Handlers.Book.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Book;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class CreateBookCommandHandlerTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private IMapper _mapper = null!;
    private BookService _bookService = null!;
    private CreateBookCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _bookService = new BookService(_bookRepository.Object);
        _sut = new CreateBookCommandHandler(_mapper, _bookService);
    }

    [Test]
    public async Task Handle_Should_PersistBookAndReturnMappedResponse_WhenCommandIsValid()
    {
        Book? captured = null;
        _bookRepository
            .Setup(r => r.InsertAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()))
            .Callback<Book, CancellationToken>((b, _) => captured = b)
            .Returns(Task.CompletedTask);

        var command = BookFaker.CreateCommand();

        var response = await _sut.Handle(command, CancellationToken.None);

        Assert.That(captured, Is.Not.Null,
            "CreateBookCommandHandler must invoke IBookRepository.InsertAsync with the new book");

        Assert.Multiple(() =>
        {
            Assert.That(captured!.Isbn, Is.EqualTo(command.Isbn),
                "Inserted Book.Isbn must mirror CreateBookCommand.Isbn");
            Assert.That(captured.Title, Is.EqualTo(command.Title),
                "Inserted Book.Title must mirror CreateBookCommand.Title");
            Assert.That(captured.AuthorId, Is.EqualTo(command.AuthorId),
                "Inserted Book.AuthorId must mirror CreateBookCommand.AuthorId");
            Assert.That(captured.NumberOfPages, Is.EqualTo(command.NumberOfPages),
                "Inserted Book.NumberOfPages must mirror CreateBookCommand.NumberOfPages");
            Assert.That(captured.PublishDate, Is.EqualTo(command.PublishDate),
                "Inserted Book.PublishDate must mirror CreateBookCommand.PublishDate");
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.Isbn, Is.EqualTo(command.Isbn),
                "BookResponse.Isbn must reflect the persisted book's ISBN");
            Assert.That(response.Title, Is.EqualTo(command.Title),
                "BookResponse.Title must reflect the persisted book's title");
            Assert.That(response.NumberOfPages, Is.EqualTo(command.NumberOfPages),
                "BookResponse.NumberOfPages must reflect the persisted book's number of pages");
            Assert.That(response.PublishDate, Is.EqualTo(command.PublishDate),
                "BookResponse.PublishDate must reflect the persisted book's publish date");
        });

        _bookRepository.Verify(
            r => r.InsertAsync(It.IsAny<Book>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "CreateBookCommandHandler must persist the new book exactly once");
    }
}
