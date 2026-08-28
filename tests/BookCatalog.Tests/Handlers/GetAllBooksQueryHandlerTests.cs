using AutoMapper;
using BookCatalog.Application.Handlers.Book.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Book.Query;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
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
    public async Task Handle_Should_ReturnEmptyItems_When_RepositoryReturnsNoBooks()
    {
        var query = new GetAllBooksQuery { Page = 1, PageSize = 10 };
        _bookRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Book>
            {
                Items = new List<Book>(),
                TotalCount = 0,
                TotalPages = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        Assert.That(response.Items, Is.Not.Null,
            "GetAllBooksResponse.Items must never be null so consumers can enumerate safely");
        Assert.That(response.Items, Is.Empty,
            "GetAllBooksResponse.Items must be empty when the repository has no books");
        Assert.Multiple(() =>
        {
            Assert.That(response.TotalCount, Is.EqualTo(0),
                "TotalCount must be zero when the repository has no matches");
            Assert.That(response.TotalPages, Is.EqualTo(0),
                "TotalPages must be zero when the repository has no matches");
            Assert.That(response.Page, Is.EqualTo(query.Page),
                "Response.Page must echo the page reported by the repository");
            Assert.That(response.PageSize, Is.EqualTo(query.PageSize),
                "Response.PageSize must echo the page size reported by the repository");
        });
    }

    [Test]
    public async Task Handle_Should_ReturnMappedBooksInSameOrder_When_RepositoryReturnsMultiple()
    {
        var first = BookFaker.Book(isbn: "1111111111", title: "First");
        var second = BookFaker.Book(isbn: "2222222222", title: "Second");
        var third = BookFaker.Book(isbn: "3333333333", title: "Third");
        var query = new GetAllBooksQuery { Page = 1, PageSize = 10 };

        _bookRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Book>
            {
                Items = new List<Book> { first, second, third },
                TotalCount = 3,
                TotalPages = 1,
                Page = query.Page,
                PageSize = query.PageSize
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        var items = response.Items.ToList();
        Assert.That(items, Has.Count.EqualTo(3),
            "GetAllBooksResponse.Items must contain one entry per repository book");
        Assert.Multiple(() =>
        {
            Assert.That(items[0].Isbn, Is.EqualTo(first.Isbn),
                "First BookResponse must correspond to the first book returned by the repository");
            Assert.That(items[1].Isbn, Is.EqualTo(second.Isbn),
                "Second BookResponse must correspond to the second book returned by the repository");
            Assert.That(items[2].Isbn, Is.EqualTo(third.Isbn),
                "Third BookResponse must correspond to the third book returned by the repository");
        });
    }

    [Test]
    public async Task Handle_Should_PropagatePaginationMetadata_When_RepositoryReturnsPagedResult()
    {
        var query = new GetAllBooksQuery { Page = 2, PageSize = 5 };
        var pagedBooks = new List<Book>
        {
            BookFaker.Book(isbn: "1000000001", title: "Sixth"),
            BookFaker.Book(isbn: "1000000002", title: "Seventh")
        };

        _bookRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Book>
            {
                Items = pagedBooks,
                TotalCount = 7,
                TotalPages = 2,
                Page = query.Page,
                PageSize = query.PageSize
            });

        var response = await _sut.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.TotalCount, Is.EqualTo(7),
                "TotalCount must be forwarded from the repository response");
            Assert.That(response.TotalPages, Is.EqualTo(2),
                "TotalPages must be forwarded from the repository response");
            Assert.That(response.Page, Is.EqualTo(query.Page),
                "Response.Page must reflect the requested page");
            Assert.That(response.PageSize, Is.EqualTo(query.PageSize),
                "Response.PageSize must reflect the requested page size");
            Assert.That(response.Items.Count(), Is.EqualTo(pagedBooks.Count),
                "Items count must match the page returned by the repository");
        });
    }

    [Test]
    public async Task Handle_Should_ForwardQueryToRepository_When_FiltersAndSortAreProvided()
    {
        var query = new GetAllBooksQuery
        {
            Title = "Pragmatic",
            Author = "Hunt",
            Isbn = "1234567890",
            SortBy = "Title",
            Desc = true,
            Page = 3,
            PageSize = 25
        };

        _bookRepository
            .Setup(r => r.GetAllAsync(query, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new BaseSearchModelPagedResponse<Book>
            {
                Items = new List<Book>(),
                TotalCount = 0,
                TotalPages = 0,
                Page = query.Page,
                PageSize = query.PageSize
            });

        await _sut.Handle(query, CancellationToken.None);

        _bookRepository.Verify(
            r => r.GetAllAsync(query, It.IsAny<CancellationToken>()),
            Times.Once,
            "Handler must forward the incoming query (filters, sort and paging) to the repository unchanged");
    }
}
