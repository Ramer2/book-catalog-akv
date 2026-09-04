using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Application.Validators.Loan.Command;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;
using FluentValidation.TestHelper;

namespace BookCatalog.Tests.Validators;

[TestFixture]
[Category("Validation")]
public class BorrowBookCommandValidatorTests
{
    private Mock<IBookRepository> _bookRepository = null!;
    private Mock<IUserRepository> _userRepository = null!;
    private Mock<ILoanService> _loanService = null!;
    private BorrowBookCommandValidator _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _bookRepository = new Mock<IBookRepository>(MockBehavior.Strict);
        _userRepository = new Mock<IUserRepository>(MockBehavior.Strict);
        _loanService = new Mock<ILoanService>(MockBehavior.Strict);

        // Default happy-path stubs. Individual tests override as needed.
        _bookRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(BookFaker.Book());
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(UserFaker.User());
        _loanService
            .Setup(s => s.EnsureBookAvailableAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        _sut = new BorrowBookCommandValidator(
            _bookRepository.Object,
            _userRepository.Object,
            _loanService.Object);
    }

    [Test]
    public async Task Should_PassValidation_When_BookAndUserExistAndBookIsAvailable()
    {
        var command = LoanFaker.BorrowCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldNotHaveAnyValidationErrors();
    }

    // ---------- BookId ----------

    [Test]
    public async Task BookId_Should_HaveRequiredError_When_Empty()
    {
        var command = LoanFaker.BorrowCommand(bookId: Guid.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.BookId)
            .WithErrorMessage("BookId is required.");
    }

    [Test]
    public async Task BookId_Should_NotInvokeExistenceCheck_When_Empty()
    {
        var command = LoanFaker.BorrowCommand(bookId: Guid.Empty);

        await _sut.TestValidateAsync(command);

        _bookRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the book-existence lookup when BookId is empty");
        _loanService.Verify(
            s => s.EnsureBookAvailableAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the availability check when BookId is empty");
    }

    [Test]
    public async Task BookId_Should_HaveNotFoundError_When_BookDoesNotExist()
    {
        _bookRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = LoanFaker.BorrowCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.BookId)
            .WithErrorMessage("Book does not exist.");
    }

    [Test]
    public async Task BookId_Should_NotCheckAvailability_When_BookDoesNotExist()
    {
        _bookRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Book?)null);

        var command = LoanFaker.BorrowCommand();

        await _sut.TestValidateAsync(command);

        _loanService.Verify(
            s => s.EnsureBookAvailableAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the availability check when the book is missing");
    }

    [Test]
    public async Task BookId_Should_HaveAvailabilityError_When_BookIsCurrentlyBorrowed()
    {
        _loanService
            .Setup(s => s.EnsureBookAvailableAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var command = LoanFaker.BorrowCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.BookId)
            .WithErrorMessage("Book is currently borrowed and cannot be lent out.");
    }

    // ---------- UserId ----------

    [Test]
    public async Task UserId_Should_HaveRequiredError_When_Empty()
    {
        var command = LoanFaker.BorrowCommand(userId: Guid.Empty);

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("UserId is required.");
    }

    [Test]
    public async Task UserId_Should_HaveNotFoundError_When_UserDoesNotExist()
    {
        _userRepository
            .Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((User?)null);

        var command = LoanFaker.BorrowCommand();

        var result = await _sut.TestValidateAsync(command);

        result.ShouldHaveValidationErrorFor(x => x.UserId)
            .WithErrorMessage("User does not exist.");
    }

    [Test]
    public async Task UserId_Should_NotInvokeExistenceCheck_When_Empty()
    {
        var command = LoanFaker.BorrowCommand(userId: Guid.Empty);

        await _sut.TestValidateAsync(command);

        _userRepository.Verify(
            r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "CascadeMode.Stop must prevent the user-existence lookup when UserId is empty");
    }
}
