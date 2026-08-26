using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Isbn;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Services;

[TestFixture]
[Category("Uniqueness")]
public class IsbnServiceTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private IsbnService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _sut = new IsbnService(_bookRepository.Object);
    }

    [Test]
    public async Task EnsureIsbnUniqueAsync_Should_ReturnTrue_When_NoBookHasThatIsbn()
    {
        _bookRepository
            .Setup(r => r.GetBookByIsbnAsync(BookFaker.ValidIsbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var isUnique = await _sut.EnsureIsbnUniqueAsync(BookFaker.ValidIsbn);

        Assert.That(isUnique, Is.True,
            "ISBN with no matching book in the repository should be considered available");
    }

    [Test]
    public async Task EnsureIsbnUniqueAsync_Should_ReturnFalse_When_IsbnTakenAndNoExclusion()
    {
        var existing = BookFaker.Book(isbn: BookFaker.ValidIsbn);
        _bookRepository
            .Setup(r => r.GetBookByIsbnAsync(BookFaker.ValidIsbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var isUnique = await _sut.EnsureIsbnUniqueAsync(BookFaker.ValidIsbn, excludeBookId: null);

        Assert.That(isUnique, Is.False,
            "When an existing book owns the ISBN and no excludeBookId is provided, the ISBN must be reported as taken");
    }

    [Test]
    public async Task EnsureIsbnUniqueAsync_Should_ReturnTrue_When_IsbnTakenByExcludedBook()
    {
        var existing = BookFaker.Book(isbn: BookFaker.ValidIsbn);
        _bookRepository
            .Setup(r => r.GetBookByIsbnAsync(BookFaker.ValidIsbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var isUnique = await _sut.EnsureIsbnUniqueAsync(BookFaker.ValidIsbn, excludeBookId: existing.Id);

        Assert.That(isUnique, Is.True,
            "When the ISBN is owned by the book being updated (excludeBookId matches), it must be considered available");
    }

    [Test]
    public async Task EnsureIsbnUniqueAsync_Should_ReturnFalse_When_IsbnTakenByDifferentBookThanExcluded()
    {
        var existing = BookFaker.Book(isbn: BookFaker.ValidIsbn);
        _bookRepository
            .Setup(r => r.GetBookByIsbnAsync(BookFaker.ValidIsbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        var otherBookId = Guid.NewGuid();

        var isUnique = await _sut.EnsureIsbnUniqueAsync(BookFaker.ValidIsbn, excludeBookId: otherBookId);

        Assert.That(isUnique, Is.False,
            "When the ISBN is owned by a book whose Id differs from excludeBookId, the ISBN must be reported as taken");
    }

    [Test]
    public async Task EnsureIsbnUniqueAsync_Should_QueryRepositoryOnce_WithSuppliedIsbnAndToken()
    {
        var token = new CancellationToken();
        _bookRepository
            .Setup(r => r.GetBookByIsbnAsync(BookFaker.ValidIsbn, token))
            .ReturnsAsync((Book?)null);

        await _sut.EnsureIsbnUniqueAsync(BookFaker.ValidIsbn, cancellationToken: token);

        _bookRepository.Verify(
            r => r.GetBookByIsbnAsync(BookFaker.ValidIsbn, token),
            Times.Once,
            "IsbnService must delegate lookup to IBookRepository.GetBookByIsbnAsync with the exact ISBN and CancellationToken it received");
    }
}
