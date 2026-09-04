using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Services;

[TestFixture]
public class LoanServiceTests
{
    private Mock<ILoanRepository> _loanRepository = null!;
    private LoanService _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loanRepository = new Mock<ILoanRepository>(MockBehavior.Strict);
        _sut = new LoanService(_loanRepository.Object);
    }

    // ---------- GetOrThrowAsync ----------

    [Test]
    public async Task GetOrThrowAsync_Should_ReturnLoan_When_RepositoryReturnsMatch()
    {
        var expected = LoanFaker.Loan();
        _loanRepository
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
        _loanRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.GetOrThrowAsync(missingId),
            "GetOrThrowAsync must throw EntityNotFoundException when the repository has no matching loan");

        Assert.That(ex!.Message, Is.EqualTo("No loan found for a given id."),
            "EntityNotFoundException message should describe the id-based lookup failure");
    }

    // ---------- BorrowAsync ----------

    [Test]
    public async Task BorrowAsync_Should_InsertNewLoanWithFreshTimestamp()
    {
        var bookId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        Loan? captured = null;

        _loanRepository
            .Setup(r => r.InsertAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .Callback<Loan, CancellationToken>((l, _) => captured = l)
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;
        var loan = await _sut.BorrowAsync(bookId, userId);
        var after = DateTime.UtcNow;

        Assert.That(captured, Is.Not.Null,
            "BorrowAsync must persist the created loan via ILoanRepository.InsertAsync");
        Assert.Multiple(() =>
        {
            Assert.That(loan, Is.SameAs(captured),
                "BorrowAsync must return the exact loan it inserted");
            Assert.That(captured!.BookId, Is.EqualTo(bookId),
                "Inserted Loan.BookId must reflect the argument");
            Assert.That(captured.UserId, Is.EqualTo(userId),
                "Inserted Loan.UserId must reflect the argument");
            Assert.That(captured.ReturnedAt, Is.Null,
                "Newly borrowed loans must have a null ReturnedAt");
            Assert.That(captured.BorrowedAt, Is.InRange(before, after),
                "BorrowedAt must be assigned to 'now' when the loan is created");
        });
    }

    // ---------- ReturnAsync ----------

    [Test]
    public async Task ReturnAsync_Should_SetReturnedAtAndSave_When_LoanIsOpen()
    {
        var openLoan = LoanFaker.Loan(returnedAt: null);
        _loanRepository
            .Setup(r => r.GetByIdAsync(openLoan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openLoan);
        _loanRepository
            .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;
        var result = await _sut.ReturnAsync(openLoan.Id);
        var after = DateTime.UtcNow;

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(openLoan),
                "ReturnAsync must return the same loan instance it mutated");
            Assert.That(openLoan.ReturnedAt, Is.Not.Null,
                "ReturnAsync must stamp ReturnedAt on the loan");
            Assert.That(openLoan.ReturnedAt!.Value, Is.InRange(before, after),
                "ReturnedAt must be assigned to 'now'");
        });

        _loanRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "ReturnAsync must persist the mutation via ILoanRepository.SaveAsync exactly once");
    }

    [Test]
    public async Task ReturnAsync_Should_BeNoop_When_LoanAlreadyReturned()
    {
        var closedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        var closedLoan = LoanFaker.Loan(returnedAt: closedAt);
        _loanRepository
            .Setup(r => r.GetByIdAsync(closedLoan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedLoan);

        var result = await _sut.ReturnAsync(closedLoan.Id);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.SameAs(closedLoan),
                "ReturnAsync must return the already-closed loan instance");
            Assert.That(closedLoan.ReturnedAt, Is.EqualTo(closedAt),
                "ReturnAsync must NOT overwrite ReturnedAt when the loan is already returned");
        });

        _loanRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "ReturnAsync must NOT call SaveAsync when the loan was already returned");
    }

    [Test]
    public void ReturnAsync_Should_ThrowEntityNotFound_When_LoanMissing()
    {
        var missingId = Guid.NewGuid();
        _loanRepository
            .Setup(r => r.GetByIdAsync(missingId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.ReturnAsync(missingId),
            "ReturnAsync must throw EntityNotFoundException when the loan is missing");

        Assert.That(ex!.Message, Is.EqualTo("No loan found for a given id."),
            "EntityNotFoundException should carry the loan-id lookup message");
    }

    // ---------- EnsureBookAvailableAsync ----------

    [Test]
    [Category("Availability")]
    public async Task EnsureBookAvailableAsync_Should_ReturnTrue_When_NoActiveLoanExists()
    {
        var bookId = Guid.NewGuid();
        _loanRepository
            .Setup(r => r.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var isAvailable = await _sut.EnsureBookAvailableAsync(bookId);

        Assert.That(isAvailable, Is.True,
            "A book without any open (unreturned) loan must be reported as available");
    }

    [Test]
    [Category("Availability")]
    public async Task EnsureBookAvailableAsync_Should_ReturnFalse_When_ActiveLoanExists()
    {
        var bookId = Guid.NewGuid();
        var openLoan = LoanFaker.Loan(bookId: bookId, returnedAt: null);
        _loanRepository
            .Setup(r => r.GetActiveLoanForBookAsync(bookId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openLoan);

        var isAvailable = await _sut.EnsureBookAvailableAsync(bookId);

        Assert.That(isAvailable, Is.False,
            "A book with an open (unreturned) loan must be reported as unavailable");
    }
}
