using AutoMapper;
using BookCatalog.Application.Handlers.Author.Query;
using BookCatalog.Application.Requests.Author.Query;
using BookCatalog.Application.Services.Author;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class GetAuthorByIdQueryHandlerTests
{
    private Mock<IAuthorService> _authorService = null!;
    private IMapper _mapper = null!;
    private GetAuthorByIdQueryHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authorService = new Mock<IAuthorService>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _sut = new GetAuthorByIdQueryHandler(_mapper, _authorService.Object);
    }

    [Test]
    public async Task Handle_Should_ReturnMappedAuthorResponse_When_AuthorExists()
    {
        var existing = AuthorFaker.Author();
        _authorService
            .Setup(s => s.GetOrThrowAsync(existing.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var query = new GetAuthorByIdQuery { Id = existing.Id };

        var response = await _sut.Handle(query, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(response.Id, Is.EqualTo(existing.Id));
            Assert.That(response.FirstName, Is.EqualTo(existing.FirstName));
            Assert.That(response.LastName, Is.EqualTo(existing.LastName));
        });
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_AuthorDoesNotExist()
    {
        var query = new GetAuthorByIdQuery { Id = Guid.NewGuid() };
        _authorService
            .Setup(s => s.GetOrThrowAsync(query.Id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("No author found for a given id."));

        Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(query, CancellationToken.None));
    }
}
