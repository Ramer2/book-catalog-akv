using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Book;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Services;

[TestFixture]
public class BookServiceTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private BookService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _sut = new BookService(_bookRepository.Object);
    }

    [Test]
    public async Task GetOrThrowAsync_Should_ReturnBook_When_RepositoryReturnsMatch()
    {
        var expected = BookFaker.Book();
        _bookRepository
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
        _bookRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.GetOrThrowAsync(missingId),
            "GetOrThrowAsync must throw EntityNotFoundException when the repository has no matching book");

        Assert.That(ex!.Message, Is.EqualTo("No book found for a given id."),
            "EntityNotFoundException message should describe the id-based lookup failure");
    }

    [Test]
    public async Task GetByIsbnOrThrowAsync_Should_ReturnBook_When_RepositoryReturnsMatch()
    {
        var expected = BookFaker.Book();
        _bookRepository
            .Setup(r => r.GetBookByIsbnAsync(expected.Isbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var actual = await _sut.GetByIsbnOrThrowAsync(expected.Isbn);

        Assert.That(actual, Is.SameAs(expected),
            "GetByIsbnOrThrowAsync must return the exact instance yielded by the repository");
    }

    [Test]
    public void GetByIsbnOrThrowAsync_Should_ThrowEntityNotFound_When_RepositoryReturnsNull()
    {
        _bookRepository
            .Setup(r => r.GetBookByIsbnAsync(BookFaker.ValidIsbn, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.GetByIsbnOrThrowAsync(BookFaker.ValidIsbn),
            "GetByIsbnOrThrowAsync must throw EntityNotFoundException when the repository has no matching book");

        Assert.That(ex!.Message, Is.EqualTo("No book found for a given isbn."),
            "EntityNotFoundException message should describe the isbn-based lookup failure");
    }

    [Test]
    public async Task CreateAsync_Should_InsertBookAndReturnIt()
    {
        var book = BookFaker.Book();
        _bookRepository
            .Setup(r => r.InsertAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var created = await _sut.CreateAsync(book);

        Assert.That(created, Is.SameAs(book),
            "CreateAsync must return the same Book instance it was given");
        _bookRepository.Verify(
            r => r.InsertAsync(book, It.IsAny<CancellationToken>()),
            Times.Once,
            "CreateAsync must persist the book via IBookRepository.InsertAsync exactly once");
    }

    [Test]
    public async Task UpdateAsync_Should_SaveAndReturnBook()
    {
        var book = BookFaker.Book();
        _bookRepository
            .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var updated = await _sut.UpdateAsync(book);

        Assert.That(updated, Is.SameAs(book),
            "UpdateAsync must return the same Book instance it was given");
        _bookRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "UpdateAsync must flush pending changes via IBookRepository.SaveAsync exactly once");
    }

    [Test]
    public async Task DeleteAsync_Should_DelegateToRepositoryDeleteEntityAsync()
    {
        var book = BookFaker.Book();
        _bookRepository
            .Setup(r => r.DeleteEntityAsync(book, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await _sut.DeleteAsync(book);

        _bookRepository.Verify(
            r => r.DeleteEntityAsync(book, It.IsAny<CancellationToken>()),
            Times.Once,
            "DeleteAsync must remove the book via IBookRepository.DeleteEntityAsync exactly once");
    }
}
