using AutoMapper;
using BookCatalog.Application.Handlers.Loan.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Requests.Loan.Command;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Domain.Exceptions;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class ReturnLoanCommandHandlerTests
{
    private Mock<ILoanRepository> _loanRepository = null!;
    private IMapper _mapper = null!;
    private LoanService _loanService = null!;
    private ReturnLoanCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loanRepository = new Mock<ILoanRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _loanService = LoanServiceFactory.Create(_loanRepository.Object);
        _sut = new ReturnLoanCommandHandler(_mapper, _loanService);
    }

    [Test]
    public async Task Handle_Should_CloseLoanAndReturnMappedResponse_WhenLoanIsOpen()
    {
        var openLoan = LoanFaker.Loan(returnedAt: null);
        _loanRepository
            .Setup(r => r.GetByIdAsync(openLoan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(openLoan);
        _loanRepository
            .Setup(r => r.SaveAsync(It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var before = DateTime.UtcNow;
        var response = await _sut.Handle(new ReturnLoanCommand { Id = openLoan.Id }, CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.Multiple(() =>
        {
            Assert.That(openLoan.ReturnedAt, Is.Not.Null,
                "Handler must stamp ReturnedAt on the loan being returned");
            Assert.That(openLoan.ReturnedAt!.Value, Is.InRange(before, after),
                "ReturnedAt must be assigned to 'now'");
            Assert.That(response.Id, Is.EqualTo(openLoan.Id),
                "LoanResponse.Id must reflect the returned loan");
            Assert.That(response.ReturnedAt, Is.EqualTo(openLoan.ReturnedAt),
                "LoanResponse.ReturnedAt must mirror the stamped domain value");
        });

        _loanRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Once,
            "ReturnLoanCommandHandler must flush changes via ILoanRepository.SaveAsync exactly once");
    }

    [Test]
    public async Task Handle_Should_BeNoop_WhenLoanAlreadyReturned()
    {
        var previouslyClosedAt = new DateTime(2026, 3, 1, 0, 0, 0, DateTimeKind.Utc);
        var closedLoan = LoanFaker.Loan(returnedAt: previouslyClosedAt);
        _loanRepository
            .Setup(r => r.GetByIdAsync(closedLoan.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(closedLoan);

        var response = await _sut.Handle(new ReturnLoanCommand { Id = closedLoan.Id }, CancellationToken.None);

        Assert.Multiple(() =>
        {
            Assert.That(closedLoan.ReturnedAt, Is.EqualTo(previouslyClosedAt),
                "Handler must NOT overwrite ReturnedAt when the loan is already returned");
            Assert.That(response.ReturnedAt, Is.EqualTo(previouslyClosedAt),
                "LoanResponse must reflect the pre-existing ReturnedAt value");
        });

        _loanRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "ReturnLoanCommandHandler must NOT call SaveAsync when the loan was already returned");
    }

    [Test]
    public void Handle_Should_ThrowEntityNotFound_When_LoanDoesNotExist()
    {
        var command = new ReturnLoanCommand { Id = Guid.NewGuid() };
        _loanRepository
            .Setup(r => r.GetByIdAsync(command.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Loan?)null);

        var ex = Assert.ThrowsAsync<EntityNotFoundException>(
            () => _sut.Handle(command, CancellationToken.None),
            "ReturnLoanCommandHandler must throw EntityNotFoundException when the target loan does not exist");

        Assert.That(ex!.Message, Is.EqualTo("No loan found for a given id."),
            "EntityNotFoundException should carry the loan-id lookup message");

        _loanRepository.Verify(
            r => r.SaveAsync(It.IsAny<CancellationToken>()),
            Times.Never,
            "ReturnLoanCommandHandler must NOT call SaveAsync when the loan cannot be found");
    }
}
