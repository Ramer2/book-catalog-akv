using BookCatalog.Application.Requests.Book.Command;
using BookCatalog.Application.Services.Author;
using BookCatalog.Application.Services.Isbn;
using BookCatalog.Application.Validators.Book.Command;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Tests.TestUtils;
using FluentValidation.TestHelper;

namespace BookCatalog.Tests.Validators;

[TestFixture]
[Category("Validation")]
public class CreateBookCommandValidatorTests
{
    private Mock<IIsbnService> _isbnService = null!;
    private Mock<IAuthorService> _authorService = null!;
    private CreateBookCommandValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _isbnService = new Mock<IIsbnService>(MockBehavior.Strict);
        _authorService = new Mock<IAuthorService>(MockBehavior.Strict);

        // Default: ISBN is available. Individual tests can override.
        _isbnService
            .Setup(s => s.EnsureIsbnUniqueAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Default: Author exists. Individual tests can override.
        _authorService
            .Setup(s => s.GetOrThrowAsync(
                It.IsAny<Guid>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(AuthorFaker.Author());

        _sut = new CreateBookCommandValidator(_isbnService.Object, _authorService.Object);
    }

    [Test]
    public async Task Should_PassValidation_When_CommandIsFullyValid()
    {
        var command = BookFaker.CreateCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Test]
    public async Task Isbn_Should_HaveRequiredError_When_Empty()
    {
        var command = BookFaker.CreateCommand(isbn: string.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Isbn)
            .WithErrorMessage("ISBN is required.");
    }

    [Test]
    public async Task Isbn_Should_NotInvokeUniquenessCheck_When_IsbnIsEmpty()
    {
        var command = BookFaker.CreateCommand(isbn: string.Empty);

        await _sut.TestValidateAsync(command);

        _isbnService.Verify(
            s => s.EnsureIsbnUniqueAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the async uniqueness check when NotEmpty fails");
    }

    [TestCase(9, TestName = "Isbn length 9 is rejected (below minimum)")]
    [TestCase(14, TestName = "Isbn length 14 is rejected (above maximum)")]
    public async Task Isbn_Should_HaveLengthError_When_OutsideAllowedRange(int length)
    {
        var command = BookFaker.CreateCommand(isbn: new string('1', length));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Isbn)
            .WithErrorMessage("ISBN must be between 10 and 13 characters long.");
    }

    [Test]
    public async Task Isbn_Should_NotInvokeUniquenessCheck_When_LengthIsInvalid()
    {
        var command = BookFaker.CreateCommand(isbn: "123");

        await _sut.TestValidateAsync(command);

        _isbnService.Verify(
            s => s.EnsureIsbnUniqueAsync(
                It.IsAny<string>(),
                It.IsAny<Guid?>(),
                It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the async uniqueness check when Length fails");
    }

    [Test]
    public async Task Isbn_Should_HaveTakenError_When_UniquenessCheckReturnsFalse()
    {
        _isbnService
            .Setup(s => s.EnsureIsbnUniqueAsync(
                BookFaker.ValidIsbn,
                null,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = BookFaker.CreateCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Isbn)
            .WithErrorMessage("ISBN is already taken.");
    }

    [Test]
    public async Task Isbn_Should_InvokeUniquenessCheck_WithNullExcludeBookId_ForCreateCommand()
    {
        var command = BookFaker.CreateCommand();

        await _sut.TestValidateAsync(command);

        _isbnService.Verify(
            s => s.EnsureIsbnUniqueAsync(
                command.Isbn,
                null,
                It.IsAny<CancellationToken>()),
            Times.Once,
            "CreateBookCommandValidator must call EnsureIsbnUniqueAsync with excludeBookId=null");
    }

    [TestCase("", TestName = "Title is required")]
    public async Task Title_Should_HaveRequiredError_When_Empty(string title)
    {
        var command = BookFaker.CreateCommand(title: title);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title is required.");
    }

    [Test]
    public async Task Title_Should_HaveLengthError_When_Exceeds256Characters()
    {
        var command = BookFaker.CreateCommand(title: new string('t', 257));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Title must not exceed 256 characters.");
    }

    [Test]
    public async Task AuthorId_Should_HaveRequiredError_When_Empty()
    {
        var command = BookFaker.CreateCommand(authorId: Guid.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage("AuthorId is required.");
    }

    [Test]
    public async Task AuthorId_Should_HaveNotFoundError_When_AuthorDoesNotExist()
    {
        var missingAuthorId = Guid.NewGuid();
        _authorService
            .Setup(s => s.GetOrThrowAsync(missingAuthorId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new EntityNotFoundException("No author found for a given id."));

        var command = BookFaker.CreateCommand(authorId: missingAuthorId);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.AuthorId)
            .WithErrorMessage("Author not found.");
    }

    [TestCase(0, TestName = "NumberOfPages 0 is rejected")]
    [TestCase(-1, TestName = "NumberOfPages -1 is rejected")]
    public async Task NumberOfPages_Should_HaveError_When_NotPositive(int pages)
    {
        var command = BookFaker.CreateCommand(numberOfPages: pages);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.NumberOfPages)
            .WithErrorMessage("Number of pages must be greater than 0.");
    }

    [Test]
    public async Task NumberOfPages_Should_NotHaveError_When_PositiveByOne()
    {
        var command = BookFaker.CreateCommand(numberOfPages: 1);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(x => x.NumberOfPages);
    }

    [Test]
    public async Task PublishDate_Should_HaveError_When_InTheFuture()
    {
        var command = BookFaker.CreateCommand(publishDate: DateTime.UtcNow.AddDays(1));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.PublishDate)
            .WithErrorMessage("Publish date cannot be in the future.");
    }

    [Test]
    public async Task PublishDate_Should_NotHaveError_When_Null()
    {
        var command = BookFaker.CreateCommand();
        command.PublishDate = null;

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PublishDate);
    }

    [Test]
    public async Task PublishDate_Should_NotHaveError_When_InThePast()
    {
        var command = BookFaker.CreateCommand(publishDate: DateTime.UtcNow.AddYears(-10));

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveValidationErrorFor(x => x.PublishDate);
    }
}
