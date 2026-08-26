using AutoMapper;
using BookCatalog.Application.Handlers.Book.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Book.Query;
using BookCatalog.Application.Services.Book;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetBookByIdQueryHandlerTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private IMapper _mapper = null!;
    private BookService _bookService = null!;
    private GetBookByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _bookService = new BookService(_bookRepository.Object);
        _sut = new GetBookByIdQueryHandler(_mapper, _bookService);
    }

    [Test]
    public async Task Handle_Should_ReturnMappedResponse_When_BookExists()
    {
        var existing = BookFaker.Book();
        _bookRepository
            .Setup(r => r.GetByIdAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var response = await _sut.Handle(new GetBookByIdQuery { Id = existing.Id }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(existing.Id), "BookResponse.Id must match the fetched book");
            Assert.That(response.Isbn, Is.EqualTo(existing.Isbn), "BookResponse.Isbn must match the fetched book");
            Assert.That(response.Title, Is.EqualTo(existing.Title), "BookResponse.Title must match the fetched book");
            Assert.That(response.Author, Is.EqualTo(existing.Author), "BookResponse.Author must match the fetched book");
            Assert.That(response.NumberOfPages, Is.EqualTo(existing.NumberOfPages),
                "BookResponse.NumberOfPages must match the fetched book");
            Assert.That(response.PublishDate, Is.EqualTo(existing.PublishDate),
                "BookResponse.PublishDate must match the fetched book");
        });
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_BookDoesNotExist()
    {
        var missingId = Guid.NewGuid();
        _bookRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(new GetBookByIdQuery { Id = missingId }, CancellationToken.None),
            "GetBookByIdQueryHandler must throw EntityNotFoundException when the book cannot be located");

        Assert.That(ex!.Message, Is.EqualTo("No book found for a given id."),
            "EntityNotFoundException should carry the id-based lookup message");
    }
}
