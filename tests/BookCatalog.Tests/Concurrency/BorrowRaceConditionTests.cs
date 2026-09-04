using BookCatalog.Application.Interfaces.Persistence;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Domain.Pagination;
using BookCatalog.Domain.SearchModels;
using Microsoft.EntityFrameworkCore;

namespace BookCatalog.Tests.Concurrency;

[TestFixture]
[Category("RaceCondition")]
public class BorrowRaceConditionTests
{
    private sealed class GatedLoanRepository : ILoanRepository
    {
        private readonly List<Loan> _loans = new();
        private readonly object _lock = new();
        private readonly TaskCompletionSource _readGate;

        public GatedLoanRepository(TaskCompletionSource readGate)
        {
            _readGate = readGate;
        }

        public IReadOnlyList<Loan> Snapshot()
        {
            lock (_lock)
            {
                return _loans.ToArray();
            }
        }

        public async Task<Loan?> GetActiveLoanForBookAsync(Guid bookId, CancellationToken cancellationToken)
        {
            await _readGate.Task.WaitAsync(cancellationToken);

            lock (_lock)
            {
                return _loans.FirstOrDefault(l => l.BookId == bookId && l.ReturnedAt == null);
            }
        }

        public Task<Loan?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                return Task.FromResult(_loans.FirstOrDefault(l => l.Id == id));
            }
        }

        public Task InsertAsync(Loan entity, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                var conflict = _loans.Any(l => l.BookId == entity.BookId && l.ReturnedAt == null);
                if (conflict)
                {
                    // Simulate the partial unique index rejecting the row.
                    throw new DbUpdateException(
                        "Simulated unique violation on UX_Loan_BookId_Active");
                }

                if (entity.Id == Guid.Empty)
                    entity.Id = Guid.NewGuid();
                _loans.Add(entity);
            }
            return Task.CompletedTask;
        }

        public Task SaveAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public Task DeleteByIdAsync(Guid id, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                var loan = _loans.FirstOrDefault(l => l.Id == id);
                if (loan != null) _loans.Remove(loan);
            }
            return Task.CompletedTask;
        }

        public Task DeleteEntityAsync(Loan entity, CancellationToken cancellationToken)
        {
            lock (_lock)
            {
                _loans.Remove(entity);
            }
            return Task.CompletedTask;
        }

        public Task<BaseSearchModelPagedResponse<Loan>> GetAllAsync(
            LoanSearchModel request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException("Not needed for this test.");

        public Task<BaseSearchModelPagedResponse<Loan>> GetLoansByUserIdAsync(
            Guid userId,
            LoanSearchModel request,
            CancellationToken cancellationToken)
            => throw new NotSupportedException("Not needed for this test.");
    }

    private static IDbExceptionInterpreter InterpreterThatFlagsDbUpdateAsUniqueViolation()
    {
        var mock = new Mock<IDbExceptionInterpreter>(MockBehavior.Strict);
        mock
            .Setup(i => i.IsUniqueViolation(It.IsAny<Exception>(), It.IsAny<string?>()))
            .Returns((Exception ex, string? _) => ex is DbUpdateException);
        return mock.Object;
    }

    [Test]
    public async Task ConcurrentBorrows_Should_ElectExactlyOneWinner_When_ActiveLoanIndexIsEnforced()
    {
        var gate = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var repo = new GatedLoanRepository(gate);
        var service = new LoanService(repo, InterpreterThatFlagsDbUpdateAsUniqueViolation());

        var bookId = Guid.NewGuid();
        var firstUserId = Guid.NewGuid();
        var secondUserId = Guid.NewGuid();

        // Interleaved borrow: check-then-write, mirroring what the validator
        // and handler do at runtime. Either the borrow succeeds and returns
        // a Loan, or it throws BookAlreadyBorrowedException because the
        // database rejected the duplicate active loan.
        async Task<(Loan? loan, BookAlreadyBorrowedException? conflict)> AttemptBorrowAsync(Guid userId)
        {
            var isAvailable = await service.EnsureBookAvailableAsync(bookId);
            if (!isAvailable) return (null, null);

            try
            {
                var loan = await service.BorrowAsync(bookId, userId);
                return (loan, null);
            }
            catch (BookAlreadyBorrowedException ex)
            {
                return (null, ex);
            }
        }

        var attempt1 = Task.Run(() => AttemptBorrowAsync(firstUserId));
        var attempt2 = Task.Run(() => AttemptBorrowAsync(secondUserId));

        // Give both tasks a moment to park on the read gate before releasing
        // them, so both observe "no active loan".
        await Task.Delay(50);
        gate.SetResult();

        var results = await Task.WhenAll(attempt1, attempt2);
        var loans = repo.Snapshot();

        var winners = results.Count(r => r.loan is not null);
        var losers = results.Count(r => r.conflict is not null);

        Assert.Multiple(() =>
        {
            Assert.That(winners, Is.EqualTo(1),
                "Exactly one concurrent borrow attempt must persist a loan");
            Assert.That(losers, Is.EqualTo(1),
                "The other concurrent borrow attempt must be rejected with BookAlreadyBorrowedException");
            Assert.That(loans.Count, Is.EqualTo(1),
                "The database must end up with a single active loan for the book");
            Assert.That(loans.Single().BookId, Is.EqualTo(bookId),
                "The surviving loan must be for the book we were trying to borrow");
            Assert.That(loans.Single().ReturnedAt, Is.Null,
                "The surviving loan must be open (ReturnedAt still null)");
        });
    }

    [Test]
    public async Task SequentialBorrows_Should_RejectSecond_When_FirstIsStillOpen()
    {
        // Sanity check with no concurrency: the second borrow either fails
        // the availability check or is rejected by the invariant on Insert.
        var alreadyOpen = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        alreadyOpen.SetResult(); // no gating needed

        var repo = new GatedLoanRepository(alreadyOpen);
        var service = new LoanService(repo, InterpreterThatFlagsDbUpdateAsUniqueViolation());

        var bookId = Guid.NewGuid();

        var isAvailable1 = await service.EnsureBookAvailableAsync(bookId);
        Assert.That(isAvailable1, Is.True,
            "The book is initially available since no loan exists");

        await service.BorrowAsync(bookId, Guid.NewGuid());

        var isAvailable2 = await service.EnsureBookAvailableAsync(bookId);
        Assert.That(isAvailable2, Is.False,
            "After a successful borrow, the book must be reported as unavailable "
            + "when the availability check runs strictly after the write");

        Assert.ThrowsAsync<BookAlreadyBorrowedException>(
            () => service.BorrowAsync(bookId, Guid.NewGuid()),
            "Even if a caller skips the availability check, the DB invariant "
            + "must still block a second concurrent-style borrow attempt");
    }
}
