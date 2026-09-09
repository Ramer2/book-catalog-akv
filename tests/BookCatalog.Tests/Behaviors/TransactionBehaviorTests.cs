using BookCatalog.Application.Behaviors;
using BookCatalog.Application.Interfaces.Transactions;
using BookCatalog.Application.Requests;
using MediatR;

namespace BookCatalog.Tests.Behaviors;

[TestFixture]
public class TransactionBehaviorTests
{
    private sealed record TransactionalRequest(string Value) : IRequest<string>, ITransactionalCommand;

    private sealed record PlainRequest(string Value) : IRequest<string>;

    private Mock<ITransactionProvider> _transactionProvider = null!;

    [SetUp]
    public void SetUp()
    {
        _transactionProvider = new Mock<ITransactionProvider>(MockBehavior.Strict);
        // Default: the provider simply forwards to the delegate. Individual
        // tests override this when they need to observe the callback.
        _transactionProvider
            .Setup(p => p.ExecuteAsync(It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()))
            .Returns((Func<Task<string>> action, CancellationToken _) => action());
    }

    [Test]
    public async Task Handle_Should_WrapNextInTransaction_When_RequestIsTransactionalCommand()
    {
        var behavior = new TransactionBehavior<TransactionalRequest, string>(_transactionProvider.Object);
        var nextInvocations = 0;

        Task<string> Next(CancellationToken _)
        {
            nextInvocations++;
            return Task.FromResult("handler-result");
        }

        var result = await behavior.Handle(new TransactionalRequest("x"), Next, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("handler-result"));
            Assert.That(nextInvocations, Is.EqualTo(1),
                "The wrapped handler must be invoked exactly once via the provider callback");
        });
        _transactionProvider.Verify(
            p => p.ExecuteAsync(It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "Transactional requests must go through the transaction provider");
    }

    [Test]
    public async Task Handle_Should_BypassTransaction_When_RequestIsNotTransactional()
    {
        var behavior = new TransactionBehavior<PlainRequest, string>(_transactionProvider.Object);
        var nextInvocations = 0;

        Task<string> Next(CancellationToken _)
        {
            nextInvocations++;
            return Task.FromResult("handler-result");
        }

        var result = await behavior.Handle(new PlainRequest("x"), Next, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(result, Is.EqualTo("handler-result"));
            Assert.That(nextInvocations, Is.EqualTo(1),
                "The handler must run directly, bypassing the transaction provider");
        });
        _transactionProvider.Verify(
            p => p.ExecuteAsync(It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()),
            Times.Never,
            "Non-transactional requests must not touch the transaction provider");
    }

    [Test]
    public void Handle_Should_PropagateException_And_InvokeNextOnce_When_HandlerThrows()
    {
        var behavior = new TransactionBehavior<TransactionalRequest, string>(_transactionProvider.Object);
        var nextInvocations = 0;
        var failure = new InvalidOperationException("boom");

        Task<string> Next(CancellationToken _)
        {
            nextInvocations++;
            throw failure;
        }

        var thrown = Assert.ThrowsAsync<InvalidOperationException>(
            () => behavior.Handle(new TransactionalRequest("x"), Next, CancellationToken.None));

        Assert.Multiple(() =>
        {
            Assert.That(thrown, Is.SameAs(failure),
                "The original exception must bubble up through the transaction provider");
            Assert.That(nextInvocations, Is.EqualTo(1),
                "The handler must still be invoked exactly once even when it throws");
        });
        _transactionProvider.Verify(
            p => p.ExecuteAsync(It.IsAny<Func<Task<string>>>(), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
