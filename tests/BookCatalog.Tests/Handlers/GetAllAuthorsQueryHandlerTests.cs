using AutoMapper;
using BookCatalog.Application.Handlers.Author.Query;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Author.Query;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetAllAuthorsQueryHandlerTests
{
    private Mock<IAuthorRepository> _authorRepository = null!;
    private IMapper _mapper = null!;
    private GetAllAuthorsQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authorRepository = new Mock<IAuthorRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new GetAllAuthorsQueryHandler(_authorRepository.Object, _mapper);
    }

    [Test]
    public async Task Handle_Should_ReturnPagedAuthorsResponse_When_Called()
    {
        var authors = new List<Author> { AuthorFaker.Author(), AuthorFaker.Author() };
        var pagedResponse = new BaseSearchModelPagedResponse<Author>
        {
            Items = authors,
            TotalCount = 2,
            TotalPages = 1,
            Page = 1,
            PageSize = 10
        };

        _authorRepository
            .Setup(r => r.GetAllAsync(It.IsAny<AuthorSearchModel>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(pagedResponse);

        var query = new GetAllAuthorsQuery();

        var response = await _sut.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.TotalCount, Is.EqualTo(2));
            Assert.That(response.Items.Count(), Is.EqualTo(2));
        });
    }
}
