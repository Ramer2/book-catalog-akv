using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Author;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Services;

[TestFixture]
public class AuthorServiceTests
{
    private Mock<IAuthorRepository> _authorRepository = null!;
    private AuthorService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _authorRepository = new Mock<IAuthorRepository>(MockBehavior.Strict);
        _sut = new AuthorService(_authorRepository.Object);
    }

    [Test]
    public async Task GetOrThrowAsync_Should_ReturnAuthor_When_RepositoryReturnsMatch()
    {
        var expected = AuthorFaker.Author();
        _authorRepository
            .Setup(r => r.GetByIdAsync(expected.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var actual = await _sut.GetOrThrowAsync(expected.Id);

        Assert.That(actual, Is.SameAs(expected),
            "GetOrThrowAsync must return the exact instance yielded by the repository");
    }

    [Test]
    public void GetOrThrowAsync_Should_ThrowEntityNotFound_When_RepositoryReturnsNull()
    {
        var missingId = Guid.NewGuid();
        _authorRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Author?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.GetOrThrowAsync(missingId),
            "GetOrThrowAsync must throw EntityNotFoundException when the repository has no matching author");

        Assert.That(ex!.Message, Is.EqualTo("No author found for a given id."),
            "EntityNotFoundException message should describe the id-based lookup failure");
    }
}
