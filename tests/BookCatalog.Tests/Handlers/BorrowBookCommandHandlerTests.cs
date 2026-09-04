using AutoMapper;
using BookCatalog.Application.Handlers.Loan.Command;
using BookCatalog.Application.Interfaces.Repositories;
using BookCatalog.Application.Services.Loan;
using BookCatalog.Domain.Models;
using BookCatalog.Tests.TestUtils;

namespace BookCatalog.Tests.Handlers;

[TestFixture]
public class BorrowBookCommandHandlerTests
{
    private Mock<ILoanRepository> _loanRepository = null!;
    private IMapper _mapper = null!;
    private LoanService _loanService = null!;
    private BorrowBookCommandHandler _sut = null!;

    [SetUp]
    public void SetUp()
    {
        _loanRepository = new Mock<ILoanRepository>(MockBehavior.Strict);
        _mapper = MapperFactory.Create();
        _loanService = LoanServiceFactory.Create(_loanRepository.Object);
        _sut = new BorrowBookCommandHandler(_mapper, _loanService);
    }

    [Test]
    public async Task Handle_Should_PersistOpenLoanAndReturnMappedResponse_WhenCommandIsValid()
    {
        Loan? captured = null;
        _loanRepository
            .Setup(r => r.InsertAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()))
            .Callback<Loan, CancellationToken>((l, _) => captured = l)
            .Returns(Task.CompletedTask);

        var command = LoanFaker.BorrowCommand();

        var before = DateTime.UtcNow;
        var response = await _sut.Handle(command, CancellationToken.None);
        var after = DateTime.UtcNow;

        Assert.That(captured, Is.Not.Null,
            "BorrowBookCommandHandler must invoke ILoanRepository.InsertAsync with the new loan");

        Assert.Multiple(() =>
        {
            Assert.That(captured!.BookId, Is.EqualTo(command.BookId),
                "Inserted Loan.BookId must mirror BorrowBookCommand.BookId");
            Assert.That(captured.UserId, Is.EqualTo(command.UserId),
                "Inserted Loan.UserId must mirror BorrowBookCommand.UserId");
            Assert.That(captured.ReturnedAt, Is.Null,
                "Newly borrowed loans must have a null ReturnedAt");
            Assert.That(captured.BorrowedAt, Is.InRange(before, after),
                "BorrowedAt must be assigned to 'now' when the loan is created");
        });

        Assert.Multiple(() =>
        {
            Assert.That(response.BookId, Is.EqualTo(command.BookId),
                "LoanResponse.BookId must reflect the persisted loan's book id");
            Assert.That(response.UserId, Is.EqualTo(command.UserId),
                "LoanResponse.UserId must reflect the persisted loan's user id");
            Assert.That(response.ReturnedAt, Is.Null,
                "LoanResponse.ReturnedAt must be null for a newly borrowed book");
        });

        _loanRepository.Verify(
            r => r.InsertAsync(It.IsAny<Loan>(), It.IsAny<CancellationToken>()),
            Times.Once,
            "BorrowBookCommandHandler must persist the new loan exactly once");
    }
}
