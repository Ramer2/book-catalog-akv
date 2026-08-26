using AutoMapper;
using BookCatalog.Application.Handlers.Book.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Book.Query;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetAllBooksQueryHandlerTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private IMapper _mapper = null!;
    private GetAllBooksQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new GetAllBooksQueryHandler(_bookRepository.Object, _mapper);
    }

    [Test]
    public async Task Handle_Should_ReturnEmptyCollection_When_RepositoryReturnsNothing()
    {
        _bookRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Book>());

        var response = await _sut.Handle(new GetAllBooksQuery(), CancellationToken.None);

        Assert.That(response.Books, Is.Not.Null,
            "GetAllBooksResponse.Books must never be null so consumers can enumerate safely");
        Assert.That(response.Books, Is.Empty,
            "GetAllBooksResponse.Books must be empty when the repository has no books");
    }

    [Test]
    public async Task Handle_Should_ReturnMappedBooksInSameOrder_When_RepositoryReturnsMultiple()
    {
        var first = BookFaker.Book(isbn: "1111111111", title: "First");
        var second = BookFaker.Book(isbn: "2222222222", title: "Second");
        var third = BookFaker.Book(isbn: "3333333333", title: "Third");
        var books = new List<Book> { first, second, third };

        _bookRepository
            .Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(books);

        var response = await _sut.Handle(new GetAllBooksQuery(), CancellationToken.None);

        Assert.That(response.Books, Has.Count.EqualTo(3),
            "GetAllBooksResponse.Books must contain one entry per repository book");
        Assert.Multiple(() =>
        {
            Assert.That(response.Books[0].Isbn, Is.EqualTo(first.Isbn),
                "First BookResponse must correspond to the first book returned by the repository");
            Assert.That(response.Books[1].Isbn, Is.EqualTo(second.Isbn),
                "Second BookResponse must correspond to the second book returned by the repository");
            Assert.That(response.Books[2].Isbn, Is.EqualTo(third.Isbn),
                "Third BookResponse must correspond to the third book returned by the repository");
        });
    }
}
